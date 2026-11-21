
using Dapper;
using SaborDeCasa.Api.Data;
using SaborDeCasa.Api.Models;

namespace SaborDeCasa.Api.Repositories;

public class PratoRepository : IPratoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PratoRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Prato>> ListarTodosAsync()
    {
        const string sql = @"
            SELECT
                p.id AS Id,
                p.nome AS Nome,
                p.preco AS Preco,
                p.disponivel AS Disponivel,
                p.categoria_id AS CategoriaId,
                c.nome AS CategoriaNome,
                p.created_at AS CreatedAt
            FROM public.pratos p
            INNER JOIN public.categoria c
                ON c.id = p.categoria_id
            ORDER BY p.nome ASC;";

        using var conexao =
            await _connectionFactory.CriarConexaoAbertaAsync();

        return await conexao.QueryAsync<Prato>(sql);
    }

    public async Task<Prato?> ObterPorIdAsync(long id)
    {
        const string sql = @"
            SELECT
                p.id AS Id,
                p.nome AS Nome,
                p.preco AS Preco,
                p.disponivel AS Disponivel,
                p.categoria_id AS CategoriaId,
                c.nome AS CategoriaNome,
                p.created_at AS CreatedAt
            FROM public.pratos p
            INNER JOIN public.categoria c
                ON c.id = p.categoria_id
            WHERE p.id = @Id;";

        using var conexao =
            await _connectionFactory.CriarConexaoAbertaAsync();

        return await conexao.QueryFirstOrDefaultAsync<Prato>(
            sql,
            new { Id = id }
        );
    }

    public async Task<IEnumerable<Prato>> ListarPorCategoriaAsync(
        long categoriaId)
    {
        const string sql = @"
            SELECT
                p.id AS Id,
                p.nome AS Nome,
                p.preco AS Preco,
                p.disponivel AS Disponivel,
                p.categoria_id AS CategoriaId,
                c.nome AS CategoriaNome,
                p.created_at AS CreatedAt
            FROM public.pratos p
            INNER JOIN public.categoria c
                ON c.id = p.categoria_id
            WHERE p.categoria_id = @CategoriaId
            ORDER BY p.nome ASC;";

        using var conexao =
            await _connectionFactory.CriarConexaoAbertaAsync();

        return await conexao.QueryAsync<Prato>(
            sql,
            new { CategoriaId = categoriaId }
        );
    }

    public async Task<Prato> CriarAsync(Prato prato)
    {
        const string sql = @"
            WITH novo_prato AS (
                INSERT INTO public.pratos
                    (nome, preco, disponivel, categoria_id)
                VALUES
                    (@Nome, @Preco, @Disponivel, @CategoriaId)
                RETURNING
                    id, nome, preco, disponivel,
                    categoria_id, created_at
            )
            SELECT
                p.id AS Id,
                p.nome AS Nome,
                p.preco AS Preco,
                p.disponivel AS Disponivel,
                p.categoria_id AS CategoriaId,
                c.nome AS CategoriaNome,
                p.created_at AS CreatedAt
            FROM novo_prato p
            INNER JOIN public.categoria c
                ON c.id = p.categoria_id;";

        using var conexao =
            await _connectionFactory.CriarConexaoAbertaAsync();

        return await conexao.QuerySingleAsync<Prato>(sql, prato);
    }

    public async Task<Prato?> AtualizarAsync(long id, Prato prato)
    {
        const string sql = @"
            WITH prato_atualizado AS (
                UPDATE public.pratos
                SET
                    nome = @Nome,
                    preco = @Preco,
                    disponivel = @Disponivel,
                    categoria_id = @CategoriaId
                WHERE id = @Id
                RETURNING
                    id, nome, preco, disponivel,
                    categoria_id, created_at
            )
            SELECT
                p.id AS Id,
                p.nome AS Nome,
                p.preco AS Preco,
                p.disponivel AS Disponivel,
                p.categoria_id AS CategoriaId,
                c.nome AS CategoriaNome,
                p.created_at AS CreatedAt
            FROM prato_atualizado p
            INNER JOIN public.categoria c
                ON c.id = p.categoria_id;";

        using var conexao =
            await _connectionFactory.CriarConexaoAbertaAsync();

        return await conexao.QueryFirstOrDefaultAsync<Prato>(
            sql,
            new
            {
                Id = id,
                prato.Nome,
                prato.Preco,
                prato.Disponivel,
                prato.CategoriaId
            }
        );
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        const string sql = @"
            DELETE FROM public.pratos
            WHERE id = @Id;";

        using var conexao =
            await _connectionFactory.CriarConexaoAbertaAsync();

        var linhasAfetadas = await conexao.ExecuteAsync(
            sql,
            new { Id = id }
        );

        return linhasAfetadas > 0;
    }

    public async Task<bool> CategoriaExisteAsync(long categoriaId)
    {
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM public.categoria
                WHERE id = @CategoriaId
            );";

        using var conexao =
            await _connectionFactory.CriarConexaoAbertaAsync();

        return await conexao.ExecuteScalarAsync<bool>(
            sql,
            new { CategoriaId = categoriaId }
        );
    }
}
