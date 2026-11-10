namespace Vezza.Infra.Queries;

public static class OrcamentoItemQuerie
{
    public static string DeleteByOrcamentoId()
    {
        return @"
                DELETE FROM OrcamentoItem
                WHERE OrcamentoId = @OrcamentoId;
            ";
    }
}
