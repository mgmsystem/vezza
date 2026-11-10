using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vezza.Domain.CQRS.Commands.Clientes;
using Vezza.Domain.CQRS.Commands.Obras;
using Vezza.Domain.CQRS.Commands.Orcamentos;
using Vezza.Domain.CQRS.Commands.Usuarios;

namespace Vezza.Tests.Commands;

[TestClass]
public class CommandValidationTests
{
    [TestMethod]
    public void CreateUsuarioCommand_ComDadosValidos_DeveSerValido()
    {
        var command = new CreateUsuarioCommand("Usuário Teste", "usuario@teste.com", "123");

        Assert.IsTrue(command.IsValid());
    }

    [TestMethod]
    public void CreateUsuarioCommand_ComEmailInvalido_DeveSerInvalido()
    {
        var command = new CreateUsuarioCommand("Usuário Teste", "email-invalido", "123");

        Assert.IsFalse(command.IsValid());
    }

    [TestMethod]
    public void CreateClienteCommand_ComDadosValidos_DeveSerValido()
    {
        var command = CreateClienteCommandValido();

        Assert.IsTrue(command.IsValid());
    }

    [TestMethod]
    public void CreateClienteCommand_SemDocumento_DeveSerInvalido()
    {
        var command = new CreateClienteCommand(
            "Cliente Teste", "", "", "", "", "", "", "", "", "");

        Assert.IsFalse(command.IsValid());
    }

    [TestMethod]
    public void CreateObraCommand_ComDadosValidos_DeveSerValido()
    {
        var command = CreateObraCommandValido();

        Assert.IsTrue(command.IsValid());
    }

    [TestMethod]
    public void CreateObraCommand_SemCliente_DeveSerInvalido()
    {
        var command = new CreateObraCommand(
            0, "Obra Teste", "", "", "", "", "", "", "");

        Assert.IsFalse(command.IsValid());
    }

    [TestMethod]
    public void CreateOrcamentoCommand_ComDadosValidos_DeveSerValido()
    {
        var command = CreateOrcamentoCommandValido();

        Assert.IsTrue(command.IsValid());
    }

    [TestMethod]
    public void CreateOrcamentoCommand_SemObjetoDaProposta_DeveSerInvalido()
    {
        var command = new CreateOrcamentoCommand(
            1, DateTime.Today, "", 100m, "À vista", "30 dias", "", "",
            DateTime.Today, DateTime.Today.AddDays(30), "Aberto");

        Assert.IsFalse(command.IsValid());
    }

    private static CreateClienteCommand CreateClienteCommandValido()
    {
        return new CreateClienteCommand(
            "Cliente Teste", "12345678900", "", "", "", "", "", "", "", "");
    }

    private static CreateObraCommand CreateObraCommandValido()
    {
        return new CreateObraCommand(
            1, "Obra Teste", "", "", "", "", "", "", "");
    }

    private static CreateOrcamentoCommand CreateOrcamentoCommandValido()
    {
        return new CreateOrcamentoCommand(
            1, DateTime.Today, "Proposta Teste", 100m, "À vista", "30 dias", "", "",
            DateTime.Today, DateTime.Today.AddDays(30), "Aberto");
    }
}
