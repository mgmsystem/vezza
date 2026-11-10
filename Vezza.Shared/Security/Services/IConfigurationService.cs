namespace Vezza.Shared.Security.Services;
public interface IConfigurationService
{
    string GetJwtKey();
    string GetApiKeyName();
    string GetApiKey();
}