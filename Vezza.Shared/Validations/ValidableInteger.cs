namespace Vezza.Shared.Validations;
public abstract partial class Validable
{
    public Validable IsRequired(int? val, string message)
    {
        if (val == null)
            validations.Add(message);

        if (val == 0)
            validations.Add(message);

        return this;
    }

    public Validable IsNotNull(int? val, string message)
    {
        if (val == null)
            validations.Add(message);

        return this;
    }

    public Validable IsGreaterThan(int val, int comparer, string message)
    {
        if (val <= (int)comparer)
            validations.Add(message);

        return this;
    }

    public Validable AreEquals(int val, int comparer, string message)
    {
        if (val != comparer)
            validations.Add(message);

        return this;
    }
}