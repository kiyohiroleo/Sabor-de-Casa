using System.Net;

namespace SaborDeCasa.Api.Exceptions;

public class RegraDeNegocioException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string Codigo { get; }
    public string? Campo { get; }

    public RegraDeNegocioException(
        string mensagem,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        string codigo = "REGRA_NEGOCIO",
        string? campo = null)
        : base(mensagem)
    {
        StatusCode = statusCode;
        Codigo = codigo;
        Campo = campo;
    }
}
