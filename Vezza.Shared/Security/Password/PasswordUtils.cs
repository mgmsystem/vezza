using SecureIdentity.Password;

namespace Vezza.Shared.Security.Password;

public static class PasswordUtils
{
    public static string PasswordEncripty(string password)
    {
        return PasswordHasher.Hash(password);
    }

    public static bool PasswordValid(string passwordBD, string password)
    {
        return PasswordHasher.Verify(passwordBD, password);
    }    
}
