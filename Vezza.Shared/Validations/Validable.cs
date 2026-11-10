namespace Vezza.Shared.Validations;
public abstract partial class Validable
{
    public List<string> validations { get; private set; } = new();

    public Validable Required()
    {
        return this;
    }

    public bool IsValid()
    {
        return validations.Count() == 0;
    }

    public List<string> Errors()
    {
        return validations;
    }

    public Validable Clear()
    {
        validations.Clear();
        return this;
    }

    public Validable SetInvalid(string message)
    {
        validations.Add(message);

        return this;
    }
}
