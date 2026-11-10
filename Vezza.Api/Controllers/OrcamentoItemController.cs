using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vezza.Domain.CQRS.Commands.OrcamentosItem;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;

namespace Vezza.Api.Controllers;

[ApiController]
public class OrcamentoItemController : ControllerBase
{
    [Authorize]
    [HttpPost("v1/orcamento-items")]
    public async Task<IActionResult> PostAsync(
        [FromBody] CreateOrcamentoItemCommand command,
        [FromServices] OrcamentoItemHandler handler)
    {
        var result = await handler.Handle(command);

        if (result.StatusCode == 201)
            return Created(result.Url, result);
        else
            return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("v1/orcamento-items")]
    public async Task<IActionResult> UpdateAsync(
        [FromBody] UpdateOrcamentoItemCommand command,
        [FromServices] OrcamentoItemHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/orcamento-items/{orcamentoItemId:int}")]
    public async Task<IActionResult> GetByIdAsync(
        [FromServices] IOrcamentoItemRepository repository,
        [FromRoute] int orcamentoItemId)
    {
        var data = await repository.GetById(orcamentoItemId);
        var result = data == null
            ? GenericResult.NotFound()
            : new GenericResult(200, data);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/orcamento-items")]
    public async Task<IActionResult> GetAllAsync(
        [FromServices] IOrcamentoItemRepository repository)
    {
        var datas = await repository.GetAll();
        var result = new GenericResult(200, datas);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("v1/orcamento-items/{orcamentoItemId:int}")]
    public async Task<IActionResult> DeleteAsync(
        [FromServices] OrcamentoItemHandler handler,
        [FromRoute] int orcamentoItemId)
    {
        var command = new DeleteOrcamentoItemCommand(orcamentoItemId);
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }
}
