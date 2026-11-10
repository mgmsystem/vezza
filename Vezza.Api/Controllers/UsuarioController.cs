using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vezza.Domain.CQRS.Commands.Usuarios;
using Vezza.Domain.CQRS.Handlers;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Repositories;

namespace Vezza.Api.Controllers;

[ApiController]
public class UsuarioController : ControllerBase
{
    [Authorize]
    [HttpPost("v1/usuarios")]
    public async Task<IActionResult> PostAsync(
                    [FromBody] CreateUsuarioCommand command,
                    [FromServices] UsuarioHandler handler)
    {
        var result = await handler.Handle(command);

        if (result.StatusCode == 201)
            return Created(result.Url, result);
        else
            return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpPut("v1/usuarios")]
    public async Task<IActionResult> UpdateAsync(
                    [FromBody] UpdateUsuarioCommand command,
                    [FromServices] UsuarioHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/usuarios/{usuarioId:int}")]
    public async Task<IActionResult> GetByIdAsync(
                    [FromServices] IUsuarioRepository repository,
                    [FromRoute] int usuarioId)
    {
        var data = await repository.GetById(usuarioId);
        if (data == null)
            return StatusCode(404, GenericResult.NotFound());

        data.Senha = null;
        var result = new GenericResult(200, data);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("v1/usuarios")]
    public async Task<IActionResult> GetAllAsync(
                    [FromServices] IUsuarioRepository repository)
    {
        var datas = await repository.GetAll();
        foreach (var usuario in datas)
            usuario.Senha = null;
        var result = new GenericResult(200, datas);
        return StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpDelete("v1/usuarios/{usuarioId:int}")]
    public async Task<IActionResult> DeleteAsync(
                    [FromServices] UsuarioHandler handler,
                    [FromRoute] int usuarioId)
    {
        var command = new DeleteUsuarioCommand(usuarioId);
        var result = await handler.Handle(command);
        
        return StatusCode(result.StatusCode, result);
    }





    [HttpPost("v1/usuarios/registrar")]
    public async Task<IActionResult> RegistrarAsync(
                    [FromBody] RegistrarCommand command,
                    [FromServices] UsuarioHandler handler)
    {
        var result = await handler.Handle(command);

        if (result.StatusCode == 201)
            return Created(result.Url, result);
        else
            return StatusCode(result.StatusCode, result);
    }

    [HttpPost("v1/usuarios/autenticar")]
    public async Task<IActionResult> AutenticarAsync(
                    [FromBody] AutenticarCommand command,
                    [FromServices] UsuarioHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("v1/usuarios/alterar-senha")]
    public async Task<IActionResult> AlterarSenhaAsync(
                    [FromBody] AlterarSenhaCommand command,
                    [FromServices] UsuarioHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("v1/usuarios/autorizacao")]
    public async Task<IActionResult> AutorizacaoAsync(
                    [FromBody] CreateAutorizacaoCommand command,
                    [FromServices] UsuarioHandler handler)
    {
        var result = await handler.Handle(command);

        return StatusCode(result.StatusCode, result);
    }
}
