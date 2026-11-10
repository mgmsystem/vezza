namespace Vezza.Infra.Queries;

public static class FluxoQuerie
{
    public static string DeleteByOrcamentoId()
    {
        return @"
                DELETE FROM Fluxo
                WHERE OrcamentoId = @OrcamentoId;
            ";
    }
}
