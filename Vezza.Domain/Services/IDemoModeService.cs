using Vezza.Domain.CQRS.Results;

namespace Vezza.Domain.Services;

public enum DemoResource
{
    Cliente,
    Obra,
    Orcamento,
    OrcamentoItem,
    Fluxo
}

public interface IDemoModeService
{
    bool IsDemoRequest { get; }
    bool PublicRegistrationEnabled { get; }
    bool IsDemoEmail(string email);
    Task<GenericResult> ExecuteCreateAsync(
        DemoResource resource,
        Func<Task<GenericResult>> createAction);
}
