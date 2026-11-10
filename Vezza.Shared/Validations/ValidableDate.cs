namespace Vezza.Shared.Validations;

public abstract partial class Validable
{
    public Validable IsRequired(DateTime? val, string message)
    {
        if (!val.HasValue)
            validations.Add(message);

        return this;
    }
}
