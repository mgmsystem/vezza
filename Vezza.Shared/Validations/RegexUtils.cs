namespace Vezza.Shared.Validations;

public static class RegexUtils
{
    public static string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    public static string UrlRegex = @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$";
    public static string OnlyNumbers = @"[^0-9]+";
    public static string OnlyLettersAndNumbers = @"[A-Za-z0-9_-]";
    public static string CpfRegex = @"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$";
    public static string CnpjRegex = @"^\d{2}\.?\d{3}\.?\d{3}/?\d{4}-?\d{2}$";
}