using System.Net;
using System.Text.Json;
using Npgsql;
using SaborDeCasa.Api.DTOs;
using SaborDeCasa.Api.Exceptions;

namespace SaborDeCasa.Api.Middlewares;

public class TratamentoDeErrosMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<TratamentoDeErrosMiddleware> _logger;

    public TratamentoDeErrosMiddleware(RequestDelegate next, ILogger<TratamentoDeErrosMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            if (!context.Response.HasStarted && context.Response.StatusCode >= 400)
            {
                await TratarStatusSemCorpoAsync(context);
            }
        }
        catch (RecursoNaoEncontradoException ex)
        {
            await EscreverErroAsync(context, HttpStatusCode.NotFound, ex.Codigo, ex.Message);
        }
        catch (RegraDeNegocioException ex)
        {
            await EscreverErroAsync(context, ex.StatusCode, ex.Codigo, ex.Message, ex.Campo);
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            await EscreverErroAsync(
                context,
                HttpStatusCode.Conflict,
                "REGISTRO_DUPLICADO",
                "Já existe um registro cadastrado com esses dados.");
        }
        catch (PostgresException ex) when (ex.SqlState == "23503")
        {
            await EscreverErroAsync(
                context,
                HttpStatusCode.Conflict,
                "VINCULO_EXISTENTE",
                "Operação bloqueada pois existem registros vinculados.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha não tratada na requisição {Method} {Path}", context.Request.Method, context.Request.Path);
            await EscreverErroAsync(
                context,
                HttpStatusCode.InternalServerError,
                "ERRO_INTERNO",
                "Erro interno ao processar a solicitação.");
        }
    }

    private static Task TratarStatusSemCorpoAsync(HttpContext context)
    {
        return context.Response.StatusCode switch
        {
            StatusCodes.Status400BadRequest => EscreverErroAsync(
                context,
                HttpStatusCode.BadRequest,
                "REQUISICAO_INVALIDA",
                "Os dados enviados na requisição são inválidos ou estão malformados."),
            StatusCodes.Status404NotFound => EscreverErroAsync(
                context,
                HttpStatusCode.NotFound,
                "ROTA_NAO_ENCONTRADA",
                "O recurso ou endpoint solicitado não foi encontrado."),
            StatusCodes.Status405MethodNotAllowed => EscreverErroAsync(
                context,
                HttpStatusCode.MethodNotAllowed,
                "METODO_NAO_PERMITIDO",
                "Método HTTP não permitido para esta rota."),
            _ => Task.CompletedTask
        };
    }

    private static async Task EscreverErroAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string codigo,
        string mensagem,
        string? campo = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var resposta = new ErroResponseDto((int)statusCode, codigo, mensagem, campo);
        var json = JsonSerializer.Serialize(resposta, JsonOptions);

        await context.Response.WriteAsync(json);
    }
}
