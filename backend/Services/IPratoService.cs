
using SaborDeCasa.Api.DTOs;

namespace SaborDeCasa.Api.Services;

public interface IPratoService
{
    Task<IEnumerable<PratoResponseDto>> ListarTodosAsync();

    Task<PratoResponseDto> ObterPorIdAsync(long id);

    Task<IEnumerable<PratoResponseDto>> ListarPorCategoriaAsync(
        long categoriaId);

    Task<PratoResponseDto> CriarAsync(CriarPratoDto dto);

    Task<PratoResponseDto> AtualizarAsync(
        long id,
        AtualizarPratoDto dto);

    Task ExcluirAsync(long id);
}
