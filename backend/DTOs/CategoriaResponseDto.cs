using SaborDeCasa.Api.Models;

namespace SaborDeCasa.Api.DTOs;

public class CategoriaResponseDto
{
    public long Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int QuantidadePratos { get; set; }
    public DateTime CreatedAt { get; set; }

    public static CategoriaResponseDto DeEntidade(Categoria categoria)
    {
        return new CategoriaResponseDto
        {
            Id = categoria.Id,
            Nome = categoria.Nome,
            QuantidadePratos = categoria.QuantidadePratos,
            CreatedAt = categoria.CreatedAt
        };
    }
}
