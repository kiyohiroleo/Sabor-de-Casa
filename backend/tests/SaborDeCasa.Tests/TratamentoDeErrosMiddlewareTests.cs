using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SaborDeCasa.Api.Exceptions;
using SaborDeCasa.Api.Middlewares;

namespace SaborDeCasa.Tests;

public class TratamentoDeErrosMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_QuandoRegraDeNegocioException_DeveRetornarPayloadCompletoParaOFront()
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();

        RequestDelegate proximo = _ => throw new RegraDeNegocioException(
            "Já existe uma categoria com o nome 'Bebidas'.",
            HttpStatusCode.Conflict,
            "CATEGORIA_DUPLICADA",
            "nome");

        var middleware = new TratamentoDeErrosMiddleware(proximo, NullLogger<TratamentoDeErrosMiddleware>.Instance);

        await middleware.InvokeAsync(contexto);

        Assert.Equal((int)HttpStatusCode.Conflict, contexto.Response.StatusCode);
        Assert.Equal("application/json", contexto.Response.ContentType);

        contexto.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(contexto.Response.Body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(409, doc.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal("CATEGORIA_DUPLICADA", doc.RootElement.GetProperty("codigo").GetString());
        Assert.Equal("nome", doc.RootElement.GetProperty("campo").GetString());
        Assert.Equal("Já existe uma categoria com o nome 'Bebidas'.", doc.RootElement.GetProperty("erro").GetString());
        Assert.Equal("Já existe uma categoria com o nome 'Bebidas'.", doc.RootElement.GetProperty("mensagem").GetString());
    }

    [Fact]
    public async Task InvokeAsync_QuandoStatus400SemCorpo_DeveRetornarJsonDeRequisicaoInvalida()
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();

        RequestDelegate proximo = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Task.CompletedTask;
        };

        var middleware = new TratamentoDeErrosMiddleware(proximo, NullLogger<TratamentoDeErrosMiddleware>.Instance);

        await middleware.InvokeAsync(contexto);

        contexto.Response.Body.Seek(0, SeekOrigin.Begin);
        var json = await new StreamReader(contexto.Response.Body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);

        Assert.Equal(400, doc.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal("REQUISICAO_INVALIDA", doc.RootElement.GetProperty("codigo").GetString());
    }
}
