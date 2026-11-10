using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vezza.Domain.CQRS.Commands.Fluxos;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;

namespace Vezza.Api.Controllers;

[ApiController]
public class FluxoController : ControllerBase
{
    [Authorize]
    [HttpPost("v1/fluxos")]
    public async Task<IActionResult> PostAsync(
                    [FromBody] CreateFluxoCommand command,
                    [FromServices] FluxoHandler handler)
    {
        var result = await handler.Handle(command);

        if (result.StatusCode == 201)
            return Created(result.Url, result);
        else
            return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("v1/fluxos")]
    public async Task<IActionResult> UpdateAsync(
                    [FromBody] UpdateFluxoCommand command,
                    [FromServices] FluxoHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/fluxos/{fluxoId:int}")]
    public async Task<IActionResult> GetByIdAsync(
                    [FromServices] IFluxoRepository repository,
                    [FromRoute] int fluxoId)
    {
        var data = await repository.GetById(fluxoId);
        var result = data == null
            ? GenericResult.NotFound()
            : new GenericResult(200, data);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/fluxos")]
    public async Task<IActionResult> GetAllAsync(
                    [FromServices] IFluxoRepository repository)
    {
        var datas = await repository.GetAll();
        var result = new GenericResult(200, datas);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("v1/fluxos/{fluxoId:int}")]
    public async Task<IActionResult> DeleteAsync(
                    [FromServices] FluxoHandler handler,
                    [FromRoute] int fluxoId)
    {
        var command = new DeleteFluxoCommand(fluxoId);
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }
}
