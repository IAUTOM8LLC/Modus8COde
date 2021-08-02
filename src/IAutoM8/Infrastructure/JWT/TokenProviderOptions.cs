using Microsoft.IdentityModel.Tokens;
using System;

namespace IAutoM8.Infrastructure.JWT
{
    public class TokenProviderOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
        public TimeSpan RememberExpiration { get; set; } = TimeSpan.FromDays(14);

        public SigningCredentials SigningCredentials { get; set; }
    }
}
