using SaborDeCasa.Api.Models;

namespace SaborDeCasa.Api.Repositories;

public class CategoriaEmMemoriaRepository : ICategoriaRepository
{
    private static readonly List<Categoria> Categorias = new()
    {
        new Categoria { Id = 1, Nome = "Marmitas Executivas", QuantidadePratos = 2, CreatedAt = DateTime.UtcNow },
        new Categoria { Id = 2, Nome = "Marmitas Fitness", QuantidadePratos = 0, CreatedAt = DateTime.UtcNow },
        new Categoria { Id = 3, Nome = "Bebidas", QuantidadePratos = 0, CreatedAt = DateTime.UtcNow }
    };

    private static long _proximoId = 4;
    private static readonly object LockObj = new();

    public Task<IEnumerable<Categoria>> ListarTodasAsync()
    {
        lock (LockObj)
        {
            var resultado = Categorias
                .OrderBy(c => c.Nome)
                .ToList()
                .AsEnumerable();
            return Task.FromResult(resultado);
        }
    }

    public Task<Categoria?> ObterPorIdAsync(long id)
    {
        lock (LockObj)
        {
            var categoria = Categorias.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(categoria);
        }
    }

    public Task<bool> ExisteNomeDuplicadoAsync(string nome, long? idIgnorado = null)
    {
        lock (LockObj)
        {
            var nomeNormalizado = nome.Trim().ToLowerInvariant();
            var existe = Categorias.Any(c =>
                c.Nome.Trim().ToLowerInvariant() == nomeNormalizado &&
                (!idIgnorado.HasValue || c.Id != idIgnorado.Value));
            return Task.FromResult(existe);
        }
    }

    public Task<bool> PossuiPratosVinculadosAsync(long categoriaId)
    {
        lock (LockObj)
        {
            var categoria = Categorias.FirstOrDefault(c => c.Id == categoriaId);
            return Task.FromResult(categoria is not null && categoria.QuantidadePratos > 0);
        }
    }

    public Task<Categoria> CriarAsync(string nome)
    {
        lock (LockObj)
        {
            var nova = new Categoria
            {
                Id = _proximoId++,
                Nome = nome,
                QuantidadePratos = 0,
                CreatedAt = DateTime.UtcNow
            };
            Categorias.Add(nova);
            return Task.FromResult(nova);
        }
    }

    public Task<Categoria?> AtualizarAsync(long id, string nome)
    {
        lock (LockObj)
        {
            var existente = Categorias.FirstOrDefault(c => c.Id == id);
            if (existente is null)
            {
                return Task.FromResult<Categoria?>(null);
            }

            existente.Nome = nome;
            return Task.FromResult<Categoria?>(existente);
        }
    }

    public Task<bool> ExcluirAsync(long id)
    {
        lock (LockObj)
        {
            var existente = Categorias.FirstOrDefault(c => c.Id == id);
            if (existente is null)
            {
                return Task.FromResult(false);
            }

            Categorias.Remove(existente);
            return Task.FromResult(true);
        }
    }
}
