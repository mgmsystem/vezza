using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vezza.Domain.Entities;
using Vezza.Infra.Repositories;

namespace Vezza.Tests.Integration;

[TestClass]
[TestCategory("Integration")]
[DoNotParallelize]
public class PersistenceIntegrationTests : MySqlIntegrationTestBase
{
    [TestMethod]
    public async Task DataContext_EfEDapperParametrizado_DevemPersistirEConsultarMapeamentos()
    {
        await using var context = CreateContext();
        Assert.IsTrue(await context.Database.CanConnectAsync());

        var cliente = new Cliente("Cliente integração", "00000000000", null, null,
            null, null, null, null, null, null);
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var obra = new Obra(cliente.ClienteId, "Obra integração", null, null,
            null, null, null, null, null);
        context.Obras.Add(obra);
        await context.SaveChangesAsync();

        var orcamento = new Orcamento
        {
            ObraId = obra.ObraId,
            Data = DateTime.UtcNow,
            ObjetoProposta = "Teste de mapeamento",
            Valor = 123.45m
        };
        context.Orcamentos.Add(orcamento);
        await context.SaveChangesAsync();

        var itemRepository = new OrcamentoItemRepository(context);
        await itemRepository.Save(new OrcamentoItem(orcamento.OrcamentoId, "Item integração", 50m));

        var connection = context.Database.GetDbConnection();
        var descricao = await connection.QuerySingleAsync<string>(
            "SELECT Descricao FROM OrcamentoItem WHERE OrcamentoId = @OrcamentoId;",
            new { orcamento.OrcamentoId });
        var persistido = await context.Orcamentos.AsNoTracking()
            .SingleAsync(x => x.OrcamentoId == orcamento.OrcamentoId);

        Assert.AreEqual("Item integração", descricao);
        Assert.AreEqual("Teste de mapeamento", persistido.ObjetoProposta);
        Assert.AreEqual(123.45m, persistido.Valor);
    }

    [TestMethod]
    public async Task TabelasTransacionais_DevemUtilizarInnoDB()
    {
        await using var context = CreateContext();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        var engines = (await connection.QueryAsync<TableEngine>(@"
            SELECT TABLE_NAME AS TableName, ENGINE AS Engine
            FROM information_schema.TABLES
            WHERE TABLE_SCHEMA = DATABASE()
              AND TABLE_NAME IN ('Usuarios', 'Autorizacao', 'Orcamento', 'OrcamentoItem', 'Fluxo');"))
            .ToList();

        Assert.AreEqual(5, engines.Count);
        Assert.IsTrue(engines.All(x => string.Equals(x.Engine, "InnoDB", StringComparison.OrdinalIgnoreCase)));
    }

    private sealed class TableEngine
    {
        public string TableName { get; set; }
        public string Engine { get; set; }
    }
}
