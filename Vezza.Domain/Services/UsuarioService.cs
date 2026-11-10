using Vezza.Domain.CQRS.Commands.Usuarios;
using Vezza.Domain.Entities;
using Vezza.Shared.Security.Password;

namespace Vezza.Domain.Services;

public static class UsuarioService
{
    public static Usuario CreateUsuarioFromCommand(CreateUsuarioCommand command)
    {
        var usuario = new Usuario();
        usuario.Nome = command.Nome;
        usuario.Email = command.Email;
        usuario.Senha = PasswordUtils.PasswordEncripty(command.Senha);
        usuario.Nivel = 0;

        return usuario;
    }

    public static Usuario CreateUsuarioFromCommand(RegistrarCommand command)
    {
        var usuario = new Usuario();
        usuario.Nome = command.Nome;
        usuario.Email = command.Email;
        usuario.Senha = PasswordUtils.PasswordEncripty(command.Senha);
        usuario.Nivel = 0;

        return usuario;
    }

    public static Usuario UpdataUsuarioFromCommand(UpdateUsuarioCommand command, Usuario data)
    {
        data.Nome = command.Nome;

        return data;
    }
}
