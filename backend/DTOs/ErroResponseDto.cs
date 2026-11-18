using System.Text.Json.Serialization;

namespace SaborDeCasa.Api.DTOs;

public class ErroResponseDto
{
    public int StatusCode { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Erro { get; set; } = string.Empty;
    public string Mensagem => Erro;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Campo { get; set; }

    public ErroResponseDto(int statusCode, string codigo, string erro, string? campo = null)
    {
        StatusCode = statusCode;
        Codigo = codigo;
        Erro = erro;
        Campo = campo;
    }
}
