using Vezza.Shared.Security.Services;

namespace Vezza.Api.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GetApiKey()
        {
            return _configuration["ApiKey"] ?? string.Empty;
        }

        public string GetApiKeyName()
        {
            return _configuration["ApiKeyName"] ?? string.Empty;
        }

        public string GetJwtKey()
        {
            return _configuration["JwtKey"] ?? string.Empty;
        }
    }
}
