namespace Vezza.Shared.Validations;

public abstract partial class Validable
{
    public Validable IsRequired(decimal? val, string message)
    {
        if (!val.HasValue)
            validations.Add(message);

        return this;
    }
}