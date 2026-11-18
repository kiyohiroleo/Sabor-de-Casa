using System.Net;
using System.Text.RegularExpressions;
using SaborDeCasa.Api.DTOs;
using SaborDeCasa.Api.Exceptions;
using SaborDeCasa.Api.Models;
using SaborDeCasa.Api.Repositories;

namespace SaborDeCasa.Api.Services;

public class CategoriaService : ICategoriaService
{
    private const int TamanhoMinimoNome = 2;
    private const int TamanhoMaximoNome = 80;

    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaService(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IEnumerable<CategoriaResponseDto>> ListarTodasAsync()
    {
        var categorias = await _categoriaRepository.ListarTodasAsync();
        return categorias.Select(CategoriaResponseDto.DeEntidade);
    }

    public async Task<CategoriaResponseDto> ObterPorIdAsync(long id)
    {
        var categoria = await GarantirCategoriaExistenteAsync(id);
        return CategoriaResponseDto.DeEntidade(categoria);
    }

    public async Task<CategoriaResponseDto> CriarAsync(CategoriaRequestDto dto)
    {
        var nomeValidado = NormalizarEValidarNome(dto?.Nome);
        await GarantirNomeUnicoAsync(nomeValidado);

        var novaCategoria = await _categoriaRepository.CriarAsync(nomeValidado);
        return CategoriaResponseDto.DeEntidade(novaCategoria);
    }

    public async Task<CategoriaResponseDto> AtualizarAsync(long id, CategoriaRequestDto dto)
    {
        await GarantirCategoriaExistenteAsync(id);

        var nomeValidado = NormalizarEValidarNome(dto?.Nome);
        await GarantirNomeUnicoAsync(nomeValidado, idIgnorado: id);

        var categoriaAtualizada = await _categoriaRepository.AtualizarAsync(id, nomeValidado);
        return CategoriaResponseDto.DeEntidade(categoriaAtualizada!);
    }

    public async Task ExcluirAsync(long id)
    {
        await GarantirCategoriaExistenteAsync(id);
        await GarantirCategoriaSemPratosAsync(id);

        await _categoriaRepository.ExcluirAsync(id);
    }

    private static string NormalizarEValidarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new RegraDeNegocioException(
                "Informe o nome da categoria.",
                HttpStatusCode.BadRequest,
                "CAMPO_OBRIGATORIO",
                "nome");
        }

        var normalizado = Regex.Replace(nome.Trim(), @"\s+", " ");

        if (normalizado.Length < TamanhoMinimoNome || normalizado.Length > TamanhoMaximoNome)
        {
            throw new RegraDeNegocioException(
                $"O nome da categoria deve ter entre {TamanhoMinimoNome} e {TamanhoMaximoNome} caracteres.",
                HttpStatusCode.BadRequest,
                "TAMANHO_INVALIDO",
                "nome");
        }

        return normalizado;
    }

    private async Task<Categoria> GarantirCategoriaExistenteAsync(long id)
    {
        if (id <= 0)
        {
            throw new RegraDeNegocioException(
                "ID da categoria inválido.",
                HttpStatusCode.BadRequest,
                "ID_INVALIDO",
                "id");
        }

        var categoria = await _categoriaRepository.ObterPorIdAsync(id);
        if (categoria is null)
        {
            throw new RecursoNaoEncontradoException("Categoria não encontrada.");
        }

        return categoria;
    }

    private async Task GarantirNomeUnicoAsync(string nome, long? idIgnorado = null)
    {
        var nomeDuplicado = await _categoriaRepository.ExisteNomeDuplicadoAsync(nome, idIgnorado);
        if (nomeDuplicado)
        {
            throw new RegraDeNegocioException(
                $"Já existe uma categoria com o nome '{nome}'.",
                HttpStatusCode.Conflict,
                "CATEGORIA_DUPLICADA",
                "nome");
        }
    }

    private async Task GarantirCategoriaSemPratosAsync(long id)
    {
        var possuiPratos = await _categoriaRepository.PossuiPratosVinculadosAsync(id);
        if (possuiPratos)
        {
            throw new RegraDeNegocioException(
                "Não é possível excluir uma categoria que possui pratos cadastrados.",
                HttpStatusCode.Conflict,
                "CATEGORIA_COM_PRATOS");
        }
    }
}
