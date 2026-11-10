namespace Vezza.Shared.Validations;
public abstract partial class Validable
{
    public Validable IsValid(Guid? val, string message)
    {
        if (val == null)
        {
            validations.Add(message);
            return this;
        }

        if (!val.HasValue)
        {
            validations.Add(message);
            return this;
        }

        if (!Guid.TryParse(val.ToString(), out var newGuid))
            validations.Add(message);

        if (val.ToString() == "00000000-0000-0000-0000-000000000000")
            validations.Add(message);

        return this;
    }
}
