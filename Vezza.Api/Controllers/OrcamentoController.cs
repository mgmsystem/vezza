using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vezza.Domain.CQRS.Commands.Orcamentos;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;

namespace Vezza.Api.Controllers;

[ApiController]
public class OrcamentoController : ControllerBase
{
    [Authorize]
    [HttpPost("v1/orcamentos")]
    public async Task<IActionResult> PostAsync(
                    [FromBody] CreateOrcamentoCommand command,
                    [FromServices] OrcamentoHandler handler)
    {
        var result = await handler.Handle(command);

        if (result.StatusCode == 201)
            return Created(result.Url, result);
        else
            return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("v1/orcamentos")]
    public async Task<IActionResult> UpdateAsync(
                    [FromBody] UpdateOrcamentoCommand command,
                    [FromServices] OrcamentoHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/orcamentos/{orcamentoId:int}")]
    public async Task<IActionResult> GetByIdAsync(
                    [FromServices] IOrcamentoRepository repository,
                    [FromRoute] int orcamentoId)
    {
        var data = await repository.GetById(orcamentoId);
        var result = data == null
            ? GenericResult.NotFound()
            : new GenericResult(200, data);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/orcamentos")]
    public async Task<IActionResult> GetAllAsync(
                    [FromServices] IOrcamentoRepository repository)
    {
        var datas = await repository.GetAll();
        var result = new GenericResult(200, datas);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("v1/orcamentos/{orcamentoId:int}")]
    public async Task<IActionResult> DeleteAsync(
                    [FromServices] OrcamentoHandler handler,
                    [FromRoute] int orcamentoId)
    {
        var command = new DeleteOrcamentoCommand(orcamentoId);
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }
}
