
namespace SaborDeCasa.Api.DTOs;

public class CriarPratoDto
{
    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public bool Disponivel { get; set; } = true;

    public long CategoriaId { get; set; }
}
