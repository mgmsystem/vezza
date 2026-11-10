using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Services;

namespace Vezza.Api.Security;

public sealed class DemoAccountProtectionMiddleware
{
    private readonly RequestDelegate _next;

    public DemoAccountProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IDemoModeService demoMode)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var isUserManagement = path.Equals("/v1/usuarios", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith("/v1/usuarios/", StringComparison.OrdinalIgnoreCase);
        var isAuthentication = path.Equals(
            "/v1/usuarios/autenticar",
            StringComparison.OrdinalIgnoreCase);

        if (context.User.Identity?.IsAuthenticated == true &&
            demoMode.IsDemoRequest &&
            isUserManagement &&
            !isAuthentication)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(
                new GenericResult(403, "A conta de demonstração não pode gerenciar usuários."));
            return;
        }

        await _next(context);
    }
}
