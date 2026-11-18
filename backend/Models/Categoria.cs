namespace SaborDeCasa.Api.Models;

public class Categoria
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadePratos { get; set; }
    public DateTime CreatedAt { get; set; }
}
