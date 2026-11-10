using System.Data;
using System.Security.Claims;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Vezza.Domain.CQRS.Results;
using Vezza.Domain.Services;
using Vezza.Infra.Context;

namespace Vezza.Api.Services;

public sealed class DemoModeService : IDemoModeService
{
    public const int RecordLimit = 5;
    public const string LimitMessage =
        "Este ambiente permite até 5 registros nesta categoria. Exclua um registro existente antes de cadastrar outro.";

    private readonly DataContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public DemoModeService(
        DataContext context,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public bool IsDemoRequest
    {
        get
        {
            var email = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
            return IsDemoEmail(email ?? string.Empty);
        }
    }

    public bool PublicRegistrationEnabled =>
        _configuration.GetValue<bool>("Security:PublicRegistrationEnabled");

    public bool IsDemoEmail(string email)
    {
        if (!_configuration.GetValue<bool>("DemoMode:Enabled") || string.IsNullOrWhiteSpace(email))
            return false;

        var configuredEmail = _configuration["DemoMode:Email"];
        return !string.IsNullOrWhiteSpace(configuredEmail) &&
               string.Equals(email.Trim(), configuredEmail.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<GenericResult> ExecuteCreateAsync(
        DemoResource resource,
        Func<Task<GenericResult>> createAction)
    {
        ArgumentNullException.ThrowIfNull(createAction);

        if (!_configuration.GetValue<bool>("DemoMode:Enabled"))
            return await createAction();

        var connection = _context.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        var lockName = $"vezza:demo:{resource}";
        var lockAcquired = false;

        try
        {
            if (openedHere)
                await connection.OpenAsync();

            var acquired = await connection.ExecuteScalarAsync<int?>(
                "SELECT GET_LOCK(@LockName, 10);",
                new { LockName = lockName });

            lockAcquired = acquired == 1;
            if (!lockAcquired)
                return new GenericResult(503, "Não foi possível reservar esta operação agora. Tente novamente.");

            if (IsDemoRequest && await CountAsync(resource) >= RecordLimit)
                return new GenericResult(409, LimitMessage);

            return await createAction();
        }
        finally
        {
            if (lockAcquired && connection.State == ConnectionState.Open)
            {
                await connection.ExecuteScalarAsync<int?>(
                    "SELECT RELEASE_LOCK(@LockName);",
                    new { LockName = lockName });
            }

            if (openedHere && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    private Task<int> CountAsync(DemoResource resource) => resource switch
    {
        DemoResource.Cliente => _context.Clientes.CountAsync(),
        DemoResource.Obra => _context.Obras.CountAsync(),
        DemoResource.Orcamento => _context.Orcamentos.CountAsync(),
        DemoResource.OrcamentoItem => _context.OrcamentoItens.CountAsync(),
        DemoResource.Fluxo => _context.Fluxos.CountAsync(),
        _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
    };
}
