namespace SaborDeCasa.Api.Exceptions;

public class RecursoNaoEncontradoException : Exception
{
    public string Codigo { get; }

    public RecursoNaoEncontradoException(string mensagem, string codigo = "NAO_ENCONTRADO")
        : base(mensagem)
    {
        Codigo = codigo;
    }
}
