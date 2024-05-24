using System.Text;
using System.Collections.Generic;
class Produto
{
    public static List<Produto> Listagem { get; set; }
    public int Codigo { get; set; }
    public string Nome { get; set; }
    static Produto()
    {
        Produto.Listagem = new List<Produto>();
        Produto.Listagem.AddRange(new List<Produto>{
            new Produto{Codigo=1, Nome="Manga"},
            new Produto{Codigo=2, Nome="Limao"},
            new Produto{Codigo=3, Nome="Mamao"},
            new Produto{Codigo=4, Nome="Uva"},
            new Produto{Codigo=5, Nome="Pera"}
        });
    }      
}