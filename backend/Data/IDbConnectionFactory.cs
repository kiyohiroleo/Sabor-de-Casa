using System.Data;

namespace SaborDeCasa.Api.Data;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CriarConexaoAbertaAsync();
}
