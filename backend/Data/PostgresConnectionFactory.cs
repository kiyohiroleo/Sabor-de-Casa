using System.Data;
using Npgsql;

namespace SaborDeCasa.Api.Data;

public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(IConfiguration configuration)
    {
        _connectionString = NormalizarConnectionString(ObterConfiguracao(configuration));
    }

    public async Task<IDbConnection> CriarConexaoAbertaAsync()
    {
        var conexao = new NpgsqlConnection(_connectionString);
        await conexao.OpenAsync();
        return conexao;
    }

    private static string ObterConfiguracao(IConfiguration configuration)
    {
        var raw = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException("String de conexão do banco não configurada.");
        }

        return raw.Trim();
    }

    public static string NormalizarConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var host = uri.Host;
        var username = Uri.UnescapeDataString(userInfo[0]);

        if (host.Equals("db.timzmuxskbuiitzcjzcm.supabase.co", StringComparison.OrdinalIgnoreCase))
        {
            host = "aws-1-us-west-2.pooler.supabase.com";
            username = "postgres.timzmuxskbuiitzcjzcm";
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = username,
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            SslMode = SslMode.Require
        };

        return builder.ToString();
    }
}
