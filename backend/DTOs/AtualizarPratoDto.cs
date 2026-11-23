
namespace SaborDeCasa.Api.DTOs;

public class AtualizarPratoDto
{
    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public bool Disponivel { get; set; }

    public long CategoriaId { get; set; }
}
