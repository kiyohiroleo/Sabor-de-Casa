
using SaborDeCasa.Api.Models;

namespace SaborDeCasa.Api.DTOs;

public class PratoResponseDto
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public bool Disponivel { get; set; }

    public long CategoriaId { get; set; }

    public string CategoriaNome { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public static PratoResponseDto DeEntidade(Prato prato)
    {
        return new PratoResponseDto
        {
            Id = prato.Id,
            Nome = prato.Nome,
            Preco = prato.Preco,
            Disponivel = prato.Disponivel,
            CategoriaId = prato.CategoriaId,
            CategoriaNome = prato.CategoriaNome,
            CreatedAt = prato.CreatedAt
        };
    }
}
