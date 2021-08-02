using IAutoM8.Global.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace IAutoM8.Infrastructure.JWT
{
    public interface ITokenProvider
    {
        object GenerateToken(ClaimsPrincipal principal, bool remember = false);
    }

    public class TokenProvider : ITokenProvider
    {
        private readonly TokenProviderOptions _options;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public TokenProvider(IOptions<JwtOptions> jwtOptions)
        {
            var jwt = jwtOptions.Value;

            var signingKey = SigningKey(jwt.JwtSecret);

            _options = new TokenProviderOptions
            {
                Audience = jwt.JwtAudience,
                Issuer = jwt.JwtIssuer,
                Expiration = jwt.JwtExpiration,
                RememberExpiration = jwt.JwtRememberExpiration,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
            };
        }

        public object GenerateToken(ClaimsPrincipal principal, bool remember = false)
        {
            if (principal?.Identity == null)
                throw new HttpRequestException("Invalid username or password.");


            var now = DateTime.UtcNow;

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            // You can add other claims here, if you want:
            var claims = principal.Claims.ToList();
            var jwtClaims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, principal.Identity.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            foreach (var claim in jwtClaims)
            {
                if (claims.Any(x => x.Type == claim.Type))
                    claims.RemoveAll(x => x.Type == claim.Type);
                claims.Add(claim);
            }

            var expiresIn = now.Add(remember ? _options.RememberExpiration : _options.Expiration);

            // Create the JWT and write it to a string
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims.ToArray(),
                notBefore: now,
                expires: expiresIn,
                signingCredentials: _options.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                expires_in = ((DateTimeOffset)expiresIn).ToUnixTimeSeconds()
            };

            return response;
        }

        public static SymmetricSecurityKey SigningKey(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }
    }
}
