using System.Data.Common;

namespace Vezza.Api.Configuration;

public static class JawsDbConnectionString
{
    public static string Create(string databaseUrl, string sslMode = "Required", int maximumPoolSize = 5)
    {
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, "mysql", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A variável JAWSDB_URL não contém uma URL MySQL válida.");

        var userInfo = uri.UserInfo.Split(':', 2);
        var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        if (userInfo.Length != 2 || string.IsNullOrWhiteSpace(uri.Host) || string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException("A variável JAWSDB_URL está incompleta.");

        var builder = new DbConnectionStringBuilder
        {
            ["Server"] = uri.Host,
            ["Port"] = uri.IsDefaultPort ? 3306 : uri.Port,
            ["Database"] = database,
            ["User Id"] = Uri.UnescapeDataString(userInfo[0]),
            ["Password"] = Uri.UnescapeDataString(userInfo[1]),
            ["SslMode"] = string.IsNullOrWhiteSpace(sslMode) ? "Required" : sslMode,
            ["Pooling"] = true,
            ["MaximumPoolSize"] = Math.Clamp(maximumPoolSize, 1, 100),
            ["ConnectionTimeout"] = 15
        };

        return builder.ConnectionString;
    }
}
