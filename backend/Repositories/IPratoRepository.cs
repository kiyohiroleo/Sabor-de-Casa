
using SaborDeCasa.Api.Models;

namespace SaborDeCasa.Api.Repositories;

public interface IPratoRepository
{
    Task<IEnumerable<Prato>> ListarTodosAsync();

    Task<Prato?> ObterPorIdAsync(long id);

    Task<IEnumerable<Prato>> ListarPorCategoriaAsync(long categoriaId);

    Task<Prato> CriarAsync(Prato prato);

    Task<Prato?> AtualizarAsync(long id, Prato prato);

    Task<bool> ExcluirAsync(long id);

    Task<bool> CategoriaExisteAsync(long categoriaId);
}
