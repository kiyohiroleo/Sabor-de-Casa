using SaborDeCasa.Api.Models;

namespace SaborDeCasa.Api.Repositories;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> ListarTodasAsync();
    Task<Categoria?> ObterPorIdAsync(long id);
    Task<bool> ExisteNomeDuplicadoAsync(string nome, long? idIgnorado = null);
    Task<bool> PossuiPratosVinculadosAsync(long categoriaId);
    Task<Categoria> CriarAsync(string nome);
    Task<Categoria?> AtualizarAsync(long id, string nome);
    Task<bool> ExcluirAsync(long id);
}
