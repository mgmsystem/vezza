using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Vezza.Shared.Security.Services;

namespace Vezza.Shared.Security.Token
{
    public class TokenUtils
    {
        private readonly IConfigurationService _configurationService;
        public TokenUtils(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }        

        public string GenerateToken(string nome, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configurationService.GetJwtKey());
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nome),
                new Claim(ClaimTypes.Email, email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        public string GerarCodigoAutorizacao()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        public string HashCodigoAutorizacao(string codigo)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(codigo));
            return Convert.ToHexString(hash);
        }

        public bool CodigoAutorizacaoValido(string codigo, string hashArmazenado)
        {
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(hashArmazenado))
                return false;

            var hashInformado = SHA256.HashData(Encoding.UTF8.GetBytes(codigo));

            try
            {
                var hashEsperado = Convert.FromHexString(hashArmazenado);
                return CryptographicOperations.FixedTimeEquals(hashInformado, hashEsperado);
            }
            catch (FormatException)
            {
                // Códigos legados em texto puro não são aceitos após a migração de segurança.
                return false;
            }
        }
    }
}
