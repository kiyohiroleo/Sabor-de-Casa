using System.Net;
using SaborDeCasa.Api.Data;
using SaborDeCasa.Api.DTOs;
using SaborDeCasa.Api.Exceptions;
using SaborDeCasa.Api.Models;
using SaborDeCasa.Api.Repositories;
using SaborDeCasa.Api.Services;

namespace SaborDeCasa.Tests;

public class CategoriaServiceTests
{
    private readonly FakeCategoriaRepository _repository;
    private readonly CategoriaService _service;

    public CategoriaServiceTests()
    {
        _repository = new FakeCategoriaRepository();
        _service = new CategoriaService(_repository);
    }

    [Fact]
    public async Task ListarTodasAsync_DeveRetornarCategoriasComQuantidadeDePratos()
    {
        var resultado = (await _service.ListarTodasAsync()).ToList();

        Assert.Equal(3, resultado.Count);
        var executivas = resultado.First(c => c.Id == 1);
        Assert.Equal(2, executivas.QuantidadePratos);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComIdExistente_DeveRetornarCategoria()
    {
        var categoria = await _service.ObterPorIdAsync(1);

        Assert.NotNull(categoria);
        Assert.Equal(1, categoria.Id);
        Assert.Equal("Marmitas Executivas", categoria.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_ComIdInexistente_DeveLancarRecursoNaoEncontradoException()
    {
        var ex = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => _service.ObterPorIdAsync(999));

        Assert.Equal("NAO_ENCONTRADO", ex.Codigo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ObterPorIdAsync_ComIdInvalido_DeveLancarRegraDeNegocioException(long idInvalido)
    {
        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.ObterPorIdAsync(idInvalido));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("ID_INVALIDO", ex.Codigo);
    }

    [Fact]
    public async Task CriarAsync_ComEspacosExtrasInternosENasPontas_DeveNormalizarNome()
    {
        var dto = new CategoriaRequestDto { Nome = "  Sobremesas    Caseiras  " };

        var criada = await _service.CriarAsync(dto);

        Assert.True(criada.Id > 0);
        Assert.Equal("Sobremesas Caseiras", criada.Nome);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public async Task CriarAsync_ComNomeVazio_DeveRetornarErroDeCampoObrigatorio(string? nomeInvalido)
    {
        var dto = new CategoriaRequestDto { Nome = nomeInvalido };

        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.CriarAsync(dto));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("CAMPO_OBRIGATORIO", ex.Codigo);
        Assert.Equal("nome", ex.Campo);
    }

    [Fact]
    public async Task CriarAsync_ComNomeMuitoCurto_DeveRetornarErroDeTamanhoInvalido()
    {
        var dto = new CategoriaRequestDto { Nome = "A" };

        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.CriarAsync(dto));

        Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
        Assert.Equal("TAMANHO_INVALIDO", ex.Codigo);
        Assert.Equal("nome", ex.Campo);
    }

    [Theory]
    [InlineData("Bebidas")]
    [InlineData("bebidas")]
    [InlineData("  BEBIDAS  ")]
    public async Task CriarAsync_ComNomeRepetido_DeveBloquearComConflict(string nomeRepetido)
    {
        var dto = new CategoriaRequestDto { Nome = nomeRepetido };

        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.CriarAsync(dto));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Equal("CATEGORIA_DUPLICADA", ex.Codigo);
        Assert.Equal("nome", ex.Campo);
    }

    [Fact]
    public async Task AtualizarAsync_ComNovoNomeValido_DeveAtualizarComSucesso()
    {
        var dto = new CategoriaRequestDto { Nome = "Bebidas Naturais" };

        var atualizada = await _service.AtualizarAsync(3, dto);

        Assert.Equal(3, atualizada.Id);
        Assert.Equal("Bebidas Naturais", atualizada.Nome);
    }

    [Fact]
    public async Task AtualizarAsync_MantendoOProprioNome_DeveFuncionarSemErroDeDuplicidade()
    {
        var dto = new CategoriaRequestDto { Nome = "Bebidas" };

        var atualizada = await _service.AtualizarAsync(3, dto);

        Assert.Equal(3, atualizada.Id);
        Assert.Equal("Bebidas", atualizada.Nome);
    }

    [Fact]
    public async Task AtualizarAsync_ComNomeDeOutraCategoriaExistente_DeveBloquearComConflict()
    {
        var dto = new CategoriaRequestDto { Nome = "Marmitas Executivas" };

        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.AtualizarAsync(3, dto));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Equal("CATEGORIA_DUPLICADA", ex.Codigo);
    }

    [Fact]
    public async Task ExcluirAsync_CategoriaComPratosVinculados_DeveBloquearExclusao()
    {
        var ex = await Assert.ThrowsAsync<RegraDeNegocioException>(
            () => _service.ExcluirAsync(1));

        Assert.Equal(HttpStatusCode.Conflict, ex.StatusCode);
        Assert.Equal("CATEGORIA_COM_PRATOS", ex.Codigo);
    }

    [Fact]
    public async Task ExcluirAsync_CategoriaVaziaSemPratos_DevePermitirExclusao()
    {
        await _service.ExcluirAsync(3);

        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
            () => _service.ObterPorIdAsync(3));
    }

    [Fact]
    public void NormalizarConnectionString_QuandoFormatoUriSupabase_DeveConverterParaNpgsql()
    {
        const string uri = "postgresql://postgres:minhaSenha123@db.exemplo.supabase.co:5432/postgres";

        var resultado = PostgresConnectionFactory.NormalizarConnectionString(uri);

        Assert.Contains("Host=db.exemplo.supabase.co", resultado);
        Assert.Contains("Username=postgres", resultado);
        Assert.Contains("Password=minhaSenha123", resultado);
        Assert.Contains("Database=postgres", resultado);
    }

    private sealed class FakeCategoriaRepository : ICategoriaRepository
    {
        private readonly List<Categoria> _categorias = new()
        {
            new Categoria { Id = 1, Nome = "Marmitas Executivas", QuantidadePratos = 2, CreatedAt = DateTime.UtcNow },
            new Categoria { Id = 2, Nome = "Marmitas Fitness", QuantidadePratos = 0, CreatedAt = DateTime.UtcNow },
            new Categoria { Id = 3, Nome = "Bebidas", QuantidadePratos = 0, CreatedAt = DateTime.UtcNow }
        };

        private long _proximoId = 4;

        public Task<IEnumerable<Categoria>> ListarTodasAsync() =>
            Task.FromResult(_categorias.OrderBy(c => c.Nome).AsEnumerable());

        public Task<Categoria?> ObterPorIdAsync(long id) =>
            Task.FromResult(_categorias.FirstOrDefault(c => c.Id == id));

        public Task<bool> ExisteNomeDuplicadoAsync(string nome, long? idIgnorado = null)
        {
            var normalizado = nome.Trim().ToLowerInvariant();
            var existe = _categorias.Any(c =>
                c.Nome.Trim().ToLowerInvariant() == normalizado &&
                (!idIgnorado.HasValue || c.Id != idIgnorado.Value));
            return Task.FromResult(existe);
        }

        public Task<bool> PossuiPratosVinculadosAsync(long categoriaId) =>
            Task.FromResult(_categorias.Any(c => c.Id == categoriaId && c.QuantidadePratos > 0));

        public Task<Categoria> CriarAsync(string nome)
        {
            var nova = new Categoria { Id = _proximoId++, Nome = nome, QuantidadePratos = 0, CreatedAt = DateTime.UtcNow };
            _categorias.Add(nova);
            return Task.FromResult(nova);
        }

        public Task<Categoria?> AtualizarAsync(long id, string nome)
        {
            var cat = _categorias.FirstOrDefault(c => c.Id == id);
            if (cat is not null) cat.Nome = nome;
            return Task.FromResult(cat);
        }

        public Task<bool> ExcluirAsync(long id)
        {
            var cat = _categorias.FirstOrDefault(c => c.Id == id);
            if (cat is null) return Task.FromResult(false);
            _categorias.Remove(cat);
            return Task.FromResult(true);
        }
    }
}
