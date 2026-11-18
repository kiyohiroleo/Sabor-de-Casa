using Dapper;
using SaborDeCasa.Api.Data;
using SaborDeCasa.Api.Models;

namespace SaborDeCasa.Api.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoriaRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Categoria>> ListarTodasAsync()
    {
        const string sql = @"
            SELECT c.id AS Id,
                   c.nome AS Nome,
                   c.created_at AS CreatedAt,
                   COUNT(p.id)::int AS QuantidadePratos
            FROM public.categoria c
            LEFT JOIN public.pratos p ON p.categoria_id = c.id
            GROUP BY c.id, c.nome, c.created_at
            ORDER BY c.nome ASC;";

        using var conexao = await _connectionFactory.CriarConexaoAbertaAsync();
        return await conexao.QueryAsync<Categoria>(sql);
    }

    public async Task<Categoria?> ObterPorIdAsync(long id)
    {
        const string sql = @"
            SELECT c.id AS Id,
                   c.nome AS Nome,
                   c.created_at AS CreatedAt,
                   COUNT(p.id)::int AS QuantidadePratos
            FROM public.categoria c
            LEFT JOIN public.pratos p ON p.categoria_id = c.id
            WHERE c.id = @Id
            GROUP BY c.id, c.nome, c.created_at;";

        using var conexao = await _connectionFactory.CriarConexaoAbertaAsync();
        return await conexao.QueryFirstOrDefaultAsync<Categoria>(sql, new { Id = id });
    }

    public async Task<bool> ExisteNomeDuplicadoAsync(string nome, long? idIgnorado = null)
    {
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM public.categoria
                WHERE LOWER(TRIM(nome)) = LOWER(TRIM(@Nome))
                  AND (@IdIgnorado IS NULL OR id <> @IdIgnorado)
            );";

        using var conexao = await _connectionFactory.CriarConexaoAbertaAsync();
        return await conexao.ExecuteScalarAsync<bool>(sql, new { Nome = nome, IdIgnorado = idIgnorado });
    }

    public async Task<bool> PossuiPratosVinculadosAsync(long categoriaId)
    {
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM public.pratos
                WHERE categoria_id = @CategoriaId
            );";

        using var conexao = await _connectionFactory.CriarConexaoAbertaAsync();
        return await conexao.ExecuteScalarAsync<bool>(sql, new { CategoriaId = categoriaId });
    }

    public async Task<Categoria> CriarAsync(string nome)
    {
        const string sql = @"
            INSERT INTO public.categoria (nome)
            VALUES (@Nome)
            RETURNING id AS Id, nome AS Nome, created_at AS CreatedAt, 0 AS QuantidadePratos;";

        using var conexao = await _connectionFactory.CriarConexaoAbertaAsync();
        return await conexao.QuerySingleAsync<Categoria>(sql, new { Nome = nome });
    }

    public async Task<Categoria?> AtualizarAsync(long id, string nome)
    {
        const string sql = @"
            UPDATE public.categoria
            SET nome = @Nome
            WHERE id = @Id;

            SELECT c.id AS Id,
                   c.nome AS Nome,
                   c.created_at AS CreatedAt,
                   COUNT(p.id)::int AS QuantidadePratos
            FROM public.categoria c
            LEFT JOIN public.pratos p ON p.categoria_id = c.id
            WHERE c.id = @Id
            GROUP BY c.id, c.nome, c.created_at;";

        using var conexao = await _connectionFactory.CriarConexaoAbertaAsync();
        return await conexao.QueryFirstOrDefaultAsync<Categoria>(sql, new { Id = id, Nome = nome });
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        const string sql = @"
            DELETE FROM public.categoria
            WHERE id = @Id;";

        using var conexao = await _connectionFactory.CriarConexaoAbertaAsync();
        var linhasAfetadas = await conexao.ExecuteAsync(sql, new { Id = id });
        return linhasAfetadas > 0;
    }
}
