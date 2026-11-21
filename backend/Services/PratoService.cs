
using System.Net;
using System.Text.RegularExpressions;
using SaborDeCasa.Api.DTOs;
using SaborDeCasa.Api.Exceptions;
using SaborDeCasa.Api.Models;
using SaborDeCasa.Api.Repositories;

namespace SaborDeCasa.Api.Services;

public class PratoService : IPratoService
{
    private readonly IPratoRepository _pratoRepository;

    private const int TamanhoMaximoNome = 150;

    public PratoService(IPratoRepository pratoRepository)
    {
        _pratoRepository = pratoRepository;
    }

    public async Task<IEnumerable<PratoResponseDto>> ListarTodosAsync()
    {
        var pratos = await _pratoRepository.ListarTodosAsync();

        return pratos.Select(PratoResponseDto.DeEntidade);
    }

    public async Task<PratoResponseDto> ObterPorIdAsync(long id)
    {
        var prato = await GarantirPratoExistenteAsync(id);

        return PratoResponseDto.DeEntidade(prato);
    }

    public async Task<IEnumerable<PratoResponseDto>> ListarPorCategoriaAsync(
        long categoriaId)
    {
        if (categoriaId <= 0)
        {
            throw new RegraDeNegocioException(
                "ID da categoria inválido.",
                HttpStatusCode.BadRequest,
                "ID_INVALIDO",
                "categoriaId");
        }

        await GarantirCategoriaExistenteAsync(categoriaId);

        var pratos = await _pratoRepository.ListarPorCategoriaAsync(categoriaId);

        return pratos.Select(PratoResponseDto.DeEntidade);
    }

    public async Task<PratoResponseDto> CriarAsync(CriarPratoDto dto)
    {
        if (dto is null)
        {
            throw new RegraDeNegocioException(
                "Informe os dados do prato.",
                HttpStatusCode.BadRequest,
                "DADOS_OBRIGATORIOS");
        }

        var nome = NormalizarEValidarNome(dto.Nome);

        ValidarPreco(dto.Preco);

        await GarantirCategoriaExistenteAsync(dto.CategoriaId);

        var prato = new Prato
        {
            Nome = nome,
            Preco = dto.Preco,
            Disponivel = dto.Disponivel,
            CategoriaId = dto.CategoriaId
        };

        var pratoCriado = await _pratoRepository.CriarAsync(prato);

        return PratoResponseDto.DeEntidade(pratoCriado);
    }

    public async Task<PratoResponseDto> AtualizarAsync(
        long id,
        AtualizarPratoDto dto)
    {
        await GarantirPratoExistenteAsync(id);

        if (dto is null)
        {
            throw new RegraDeNegocioException(
                "Informe os dados do prato.",
                HttpStatusCode.BadRequest,
                "DADOS_OBRIGATORIOS");
        }

        var nome = NormalizarEValidarNome(dto.Nome);

        ValidarPreco(dto.Preco);

        await GarantirCategoriaExistenteAsync(dto.CategoriaId);

        var prato = new Prato
        {
            Nome = nome,
            Preco = dto.Preco,
            Disponivel = dto.Disponivel,
            CategoriaId = dto.CategoriaId
        };

        var pratoAtualizado = await _pratoRepository.AtualizarAsync(id, prato);

        if (pratoAtualizado is null)
        {
            throw new RecursoNaoEncontradoException("Prato não encontrado.");
        }

        return PratoResponseDto.DeEntidade(pratoAtualizado);
    }

    public async Task ExcluirAsync(long id)
    {
        await GarantirPratoExistenteAsync(id);

        var excluido = await _pratoRepository.ExcluirAsync(id);

        if (!excluido)
        {
            throw new RecursoNaoEncontradoException("Prato não encontrado.");
        }
    }

    private async Task<Prato> GarantirPratoExistenteAsync(long id)
    {
        if (id <= 0)
        {
            throw new RegraDeNegocioException(
                "ID do prato inválido.",
                HttpStatusCode.BadRequest,
                "ID_INVALIDO",
                "id");
        }

        var prato = await _pratoRepository.ObterPorIdAsync(id);

        if (prato is null)
        {
            throw new RecursoNaoEncontradoException("Prato não encontrado.");
        }

        return prato;
    }

    private async Task GarantirCategoriaExistenteAsync(long categoriaId)
    {
        if (categoriaId <= 0)
        {
            throw new RegraDeNegocioException(
                "Selecione uma categoria válida.",
                HttpStatusCode.BadRequest,
                "ID_INVALIDO",
                "categoriaId");
        }

        var existe = await _pratoRepository.CategoriaExisteAsync(categoriaId);

        if (!existe)
        {
            throw new RecursoNaoEncontradoException("Categoria não encontrada.");
        }
    }

    private static string NormalizarEValidarNome(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new RegraDeNegocioException(
                "Informe o nome do prato.",
                HttpStatusCode.BadRequest,
                "CAMPO_OBRIGATORIO",
                "nome");
        }

        var normalizado = Regex.Replace(nome.Trim(), @"\s+", " ");

        if (normalizado.Length > TamanhoMaximoNome)
        {
            throw new RegraDeNegocioException(
                $"O nome do prato deve ter no máximo {TamanhoMaximoNome} caracteres.",
                HttpStatusCode.BadRequest,
                "TAMANHO_INVALIDO",
                "nome");
        }

        return normalizado;
    }

    private static void ValidarPreco(decimal preco)
    {
        if (preco <= 0)
        {
            throw new RegraDeNegocioException(
                "O preço do prato deve ser maior que zero.",
                HttpStatusCode.BadRequest,
                "PRECO_INVALIDO",
                "preco");
        }

        if (preco > 99999999.99m)
        {
            throw new RegraDeNegocioException(
                "O preço informado excede o limite permitido.",
                HttpStatusCode.BadRequest,
                "PRECO_INVALIDO",
                "preco");
        }
    }
}
