using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Vezza.Domain.CQRS.Commands.Orcamentos;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.Entities;
using Vezza.Infra.Context;
using Vezza.Infra.Repositories;

namespace Vezza.Tests.Integration;

[TestClass]
[TestCategory("Integration")]
[DoNotParallelize]
public class OrcamentoTransactionIntegrationTests : MySqlIntegrationTestBase
{
    [TestMethod]
    public async Task ExcluirOrcamento_DeveRemoverOrcamentoItensEFluxosNaMesmaTransacao()
    {
        int orcamentoId;
        await using (var preparationContext = CreateContext())
        {
            orcamentoId = await SeedOrcamento(preparationContext);
        }

        await using (var executionContext = CreateContext())
        {
            var handler = CreateHandler(executionContext);
            var result = await handler.Handle(new DeleteOrcamentoCommand(orcamentoId));
            Assert.AreEqual(200, result.StatusCode);
        }

        await using var verificationContext = CreateContext();
        Assert.AreEqual(0, await CountRows(verificationContext, "Orcamento", orcamentoId));
        Assert.AreEqual(0, await CountRows(verificationContext, "OrcamentoItem", orcamentoId));
        Assert.AreEqual(0, await CountRows(verificationContext, "Fluxo", orcamentoId));
    }

    [TestMethod]
    public async Task ExcluirOrcamento_ComFalhaIntermediaria_DeveReverterExclusaoDapper()
    {
        int orcamentoId;
        await using (var preparationContext = CreateContext())
        {
            orcamentoId = await SeedOrcamento(preparationContext);
            var connection = preparationContext.Database.GetDbConnection();
            await connection.OpenAsync();
            await connection.ExecuteAsync(@"
                CREATE TRIGGER FalharExclusaoFluxo
                BEFORE DELETE ON Fluxo
                FOR EACH ROW
                SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Falha controlada de integração';");

            var triggerCount = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*)
                FROM information_schema.TRIGGERS
                WHERE TRIGGER_SCHEMA = DATABASE()
                  AND TRIGGER_NAME = 'FalharExclusaoFluxo';");
            Assert.AreEqual(1, triggerCount);
        }

        await using (var executionContext = CreateContext())
        {
            var handler = CreateHandler(executionContext);
            var result = await handler.Handle(new DeleteOrcamentoCommand(orcamentoId));
            Assert.AreEqual(500, result.StatusCode);
        }

        await using var verificationContext = CreateContext();
        Assert.AreEqual(1, await CountRows(verificationContext, "Orcamento", orcamentoId));
        Assert.AreEqual(1, await CountRows(verificationContext, "OrcamentoItem", orcamentoId));
        Assert.AreEqual(1, await CountRows(verificationContext, "Fluxo", orcamentoId));
    }

    private static OrcamentoHandler CreateHandler(DataContext context)
    {
        return new OrcamentoHandler(
            new OrcamentoRepository(context),
            new OrcamentoItemRepository(context),
            new FluxoRepository(context),
            new UnitOfWork(context),
            NullLogger<OrcamentoHandler>.Instance);
    }

    private static async Task<int> SeedOrcamento(DataContext context)
    {
        var cliente = new Cliente("Cliente transação", "11111111111", null, null,
            null, null, null, null, null, null);
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        var obra = new Obra(cliente.ClienteId, "Obra transação", null, null,
            null, null, null, null, null);
        context.Obras.Add(obra);
        await context.SaveChangesAsync();
        var orcamento = new Orcamento { ObraId = obra.ObraId, Data = DateTime.UtcNow };
        context.Orcamentos.Add(orcamento);
        await context.SaveChangesAsync();
        context.OrcamentoItens.Add(new OrcamentoItem(orcamento.OrcamentoId, "Item", 10m));
        context.Fluxos.Add(new Fluxo { OrcamentoId = orcamento.OrcamentoId, Descricao = "Fluxo", Valor = 10m });
        await context.SaveChangesAsync();
        return orcamento.OrcamentoId;
    }

    private static async Task<int> CountRows(DataContext context, string table, int orcamentoId)
    {
        var connection = context.Database.GetDbConnection();
        return await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {table} WHERE OrcamentoId = @OrcamentoId;",
            new { OrcamentoId = orcamentoId });
    }
}
