using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Vezza.Infra.Context;

namespace Vezza.Tests.Integration;

public abstract class MySqlIntegrationTestBase
{
    protected const string ConnectionVariable = "VEZZA_TEST_MYSQL_CONNECTION";
    protected string ConnectionString { get; private set; }

    [TestInitialize]
    public async Task InitializeDatabase()
    {
        ConnectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            Assert.Inconclusive(
                $"Defina {ConnectionVariable} para executar os testes contra MySQL real.");
        }

        await CleanupDatabase();
    }

    [TestCleanup]
    public async Task CleanupDatabaseAfterTest()
    {
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            await CleanupDatabase();
        }
    }

    protected DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseMySQL(ConnectionString, null)
            .Options;

        return new DataContext(options);
    }

    private async Task CleanupDatabase()
    {
        await using var context = CreateContext();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync("DROP TRIGGER IF EXISTS FalharExclusaoFluxo;");
        await connection.ExecuteAsync("DELETE FROM Fluxo;");
        await connection.ExecuteAsync("DELETE FROM OrcamentoItem;");
        await connection.ExecuteAsync("DELETE FROM Orcamento;");
        await connection.ExecuteAsync("DELETE FROM Obra;");
        await connection.ExecuteAsync("DELETE FROM Cliente;");
        await connection.ExecuteAsync("DELETE FROM Autorizacao;");
        await connection.ExecuteAsync("DELETE FROM Usuarios;");
    }
}
