using System.Text.RegularExpressions;

namespace Vezza.Shared.Validations;
public abstract partial class Validable
{
    public Validable IsNotNull(string val, string message)
    {
        if (val == null)
            validations.Add(message);

        return this;
    }

    public Validable IsNull(string val, string message)
    {
        if (val != null)
            validations.Add(message);

        return this;
    }

    public Validable IsNotNullOrEmpty(string val, string message)
    {
        if (string.IsNullOrEmpty(val))
            validations.Add(message);

        return this;
    }
    public Validable IsGreaterThan(string val, int comparer, string message)
    {
        if (val == null)
        {
            validations.Add(message);
            return this;
        }

        if (val.Length <= comparer)
            validations.Add(message);

        return this;
    }

    public Validable AreEquals(string val, string comparer, string message)
    {
        if (val == null)
        {
            validations.Add(message);
            return this;
        }

        if (comparer == null)
        {
            validations.Add(message);
            return this;
        }

        if (!val.Equals(comparer))
            validations.Add(message);

        return this;
    }

    public Validable IsInList(string val, List<string> possibilities, string message)
    {
        if (!possibilities.Contains(val))
        {
            validations.Add(message);
        }

        return this;
    }


    public Validable IsEmail(string val, string message)
    {
        if (!Regex.IsMatch(val ?? "", RegexUtils.EmailRegex))
            validations.Add(message);

        return this;
    }

    public Validable IsCreditCard(string val, string message)
    {
        val = Regex.Replace(val, RegexUtils.OnlyNumbers, "");

        if (string.IsNullOrWhiteSpace(val))
        {
            validations.Add(message);
            return this;
        }

        var even = false;
        var checksum = 0;

        foreach (var digit in val.ToCharArray().Reverse())
        {
            if (!char.IsDigit(digit))
            {
                validations.Add(message);
                return this;
            }

            var value = (digit - '0') * (even ? 2 : 1);
            even = !even;

            while (value > 0)
            {
                checksum += value % 10;
                value /= 10;
            }
        }

        if (checksum % 10 != 0)
            validations.Add(message);

        return this;
    }
}
