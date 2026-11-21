
namespace SaborDeCasa.Api.Models;

public class Prato
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public bool Disponivel { get; set; } = true;

    public long CategoriaId { get; set; }

    public string CategoriaNome { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
