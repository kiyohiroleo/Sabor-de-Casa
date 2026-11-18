using SaborDeCasa.Api.DTOs;

namespace SaborDeCasa.Api.Services;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaResponseDto>> ListarTodasAsync();
    Task<CategoriaResponseDto> ObterPorIdAsync(long id);
    Task<CategoriaResponseDto> CriarAsync(CategoriaRequestDto dto);
    Task<CategoriaResponseDto> AtualizarAsync(long id, CategoriaRequestDto dto);
    Task ExcluirAsync(long id);
}
