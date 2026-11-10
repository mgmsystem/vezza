using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vezza.Domain.CQRS.Commands.Clientes;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;

namespace Vezza.Api.Controllers;

[ApiController]
public class ClienteController : ControllerBase
{
    [Authorize]
    [HttpPost("v1/clientes")]
    public async Task<IActionResult> PostAsync(
                    [FromBody] CreateClienteCommand command,
                    [FromServices] ClienteHandler handler)
    {
        var result = await handler.Handle(command);

        if (result.StatusCode == 201)
            return Created(result.Url, result);
        else
            return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("v1/clientes")]
    public async Task<IActionResult> UpdateAsync(
                    [FromBody] UpdateClienteCommand command,
                    [FromServices] ClienteHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/clientes/{clienteId:int}")]
    public async Task<IActionResult> GetByIdAsync(
                    [FromServices] IClienteRepository repository,
                    [FromRoute] int clienteId)
    {
        var data = await repository.GetById(clienteId);
        var result = data == null
            ? GenericResult.NotFound()
            : new GenericResult(200, data);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/clientes")]
    public async Task<IActionResult> GetAllAsync(
                    [FromServices] IClienteRepository repository)
    {
        var datas = await repository.GetAll();
        var result = new GenericResult(200, datas);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("v1/clientes/{clienteId:int}")]
    public async Task<IActionResult> ClearAsync(
                    [FromServices] ClienteHandler handler,
                    [FromRoute] int clienteId)
    {
        var command = new DeleteClienteCommand(clienteId);
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }
}
