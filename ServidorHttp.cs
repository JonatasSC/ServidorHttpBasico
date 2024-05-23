using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System;
using System.IO;

class ServidorHttp
{
    private TcpListener Controlador { get; set; } // responsavel por escutar uma porta de rede e esperar a solicitacao de conexao TCP
    private int Porta { get; set; } // Contem o numero da porta que vai ser escutada
    private int QtdeRequests { get; set; }// contador que ajuda a ver se alguma requisicao foi perdida
    public string HtmlExemplo { get; set; } 

    public ServidorHttp(int porta = 8080) // Construtor com uma porta padrao
    {
        this.Porta = porta;
        this.CriarHtmloExemplo();//metodo que gera o HTML
        try{
            this.Controlador = new TcpListener(IPAddress.Parse("127.0.0.1"), this.Porta);
            this.Controlador.Start(); //Iniciando a escuta na porta 8080
            System.Console.WriteLine($"Servidor HTTP esta rodando na porta {this.Porta}.");
            System.Console.WriteLine($"Para acessar, digite no navegador: http://localhost:{this.Porta}.");
            Task servidorHttpTask = Task.Run(() => AguardarRequests());
            servidorHttpTask.GetAwaiter().GetResult();//Faz com que o programa agurde o termino do metodo aguardar request
        }
        catch (Exception e)
        {
            System.Console.WriteLine($"Erro ao iniciar servidor na porta {this.Porta}:\n{e.Message}");
        }
    }

    private async Task AguardarRequests()//Metodo responsavel por aguardar as requisicoes e retornar uma resposta
    {
        while (true)
        {
            Socket conexao = await this.Controlador.AcceptSocketAsync();
            this.QtdeRequests++;
            Task task = Task.Run(() => ProcessarRequest(conexao, this.QtdeRequests));//Fazendo uma chamada assíncrona ao metodo processar request
        }
    }

    private void ProcessarRequest(Socket conexao, int numeroRequest) // metodo responsavel por processar a requisicao
    {
        System.Console.WriteLine($"Processando request #{numeroRequest}...\n");
        if (conexao.Connected)
        {
            byte[] bytesRequisicao = new byte[1024];
            conexao.Receive(bytesRequisicao, bytesRequisicao.Length, 0);//preenche o vetor de bytes com os dados recebidos
            string textoRequisicao = Encoding.UTF8.GetString(bytesRequisicao)
                .Replace((char)0, ' ').Trim(); // Limpando a memoria removendo os zeros que sobram por espacos vazios
            if (textoRequisicao.Length > 0)
            {
                System.Console.WriteLine($"\n{textoRequisicao}\n");

                string[] linhas = textoRequisicao.Split("\r\n");
                int iPrimeiroEspaco = linhas[0].IndexOf(' ');
                int iSegundoEspaco = linhas[0].LastIndexOf(' ');
                string metodoHttp = linhas[0].Substring(0, iPrimeiroEspaco);
                string recursoBuscado = linhas[0].Substring(iPrimeiroEspaco + 1,
                    iSegundoEspaco - iPrimeiroEspaco - 1);
                string versaoHttp = linhas[0].Substring(iSegundoEspaco + 1);
                iPrimeiroEspaco = linhas[1].IndexOf(' ');
                string nomeHost = linhas[1].Substring(iPrimeiroEspaco + 1);

                byte[] bytesCabecalho = null;
                var bytesConteudo = LerArquivo(recursoBuscado);
                if (bytesConteudo.Length > 0)
                {
                    bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8",
                        "200", bytesConteudo.Length);
                }
                else
                {
                    bytesConteudo = Encoding.UTF8.GetBytes(
                        "<h1>Erro 404 - Arquivo nao Encontrado</h1>");
                    bytesCabecalho = GerarCabecalho(versaoHttp, "text/html;charset=utf-8",
                        "404", bytesConteudo.Length);
                }
                int bytesEnviados = conexao.Send(bytesCabecalho, bytesCabecalho.Length, 0);
                bytesEnviados += conexao.Send(bytesConteudo, bytesConteudo.Length, 0);
                conexao.Close();
                System.Console.WriteLine($"\n{bytesEnviados} bytes enviados em resposta a requisicao # {numeroRequest}.");
            }
        }
        System.Console.WriteLine($"\nRequest {numeroRequest} finalizado.");
    }

    public byte[] GerarCabecalho(string versaoHttp, string tipoMime, //Metodo para gerar um cabecalho de resposta
        string codigoHttp, int qtdeBytes = 0)
        {
            StringBuilder text = new StringBuilder();
            text.Append($"{versaoHttp} {codigoHttp} {Environment.NewLine}");
            text.Append($"Server: ServidorHttp Http Simples 1.0{Environment.NewLine}");
            text.Append($"Content-Type {tipoMime}{Environment.NewLine}");
            text.Append($"Content-Length: {qtdeBytes}{Environment.NewLine}{Environment.NewLine}");
            return Encoding.UTF8.GetBytes(text.ToString()); //Convertendo todo o texto para uma unica string e depois transforma para bytes
        }

        private void CriarHtmloExemplo ()
        {
            StringBuilder html = new StringBuilder();
            html.Append("<!DOCTYPE html><html lang=\"pt-br\"><head><meta charset=\"UTF-8\">");
            html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.Append("<title>Pagina Estatica</title></head><body>");
            html.Append("<h1>Pagina Estatica</h1></body></html>");
            this.HtmlExemplo = html.ToString();
        }

        public byte[] LerArquivo(string recurso)//Metodo de leitura de arquivos
        {
            string diretorio = "//home//Jonatas//Programs//ServidorHttpBasico//www";
            string caminhoArquivo = diretorio + recurso.Replace("/", "//");
            if (File.Exists(caminhoArquivo))//Verificando a existencia do arquivo
            {
                return File.ReadAllBytes(caminhoArquivo);
            }
            else return new byte[0];
        }
}