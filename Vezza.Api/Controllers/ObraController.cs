using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vezza.Domain.CQRS.Commands.Obras;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;

namespace Vezza.Api.Controllers;

[ApiController]
public class ObraController : ControllerBase
{
    [Authorize]
    [HttpPost("v1/obras")]
    public async Task<IActionResult> PostAsync(
                    [FromBody] CreateObraCommand command,
                    [FromServices] ObraHandler handler)
    {
        var result = await handler.Handle(command);

        if (result.StatusCode == 201)
            return Created(result.Url, result);
        else
            return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("v1/obras")]
    public async Task<IActionResult> UpdateAsync(
                    [FromBody] UpdateObraCommand command,
                    [FromServices] ObraHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/obras/{obraId:int}")]
    public async Task<IActionResult> GetByIdAsync(
                    [FromServices] IObraRepository repository,
                    [FromRoute] int obraId)
    {
        var data = await repository.GetById(obraId);
        var result = data == null
            ? GenericResult.NotFound()
            : new GenericResult(200, data);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/obras")]
    public async Task<IActionResult> GetAllAsync(
                    [FromServices] IObraRepository repository)
    {
        var datas = await repository.GetAll();
        var result = new GenericResult(200, datas);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("v1/obras/{obraId:int}")]
    public async Task<IActionResult> DeleteAsync(
                    [FromServices] ObraHandler handler,
                    [FromRoute] int obraId)
    {
        var command = new DeleteObraCommand(obraId);
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }
}
