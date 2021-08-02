using System;

namespace IAutoM8.Global.Options
{
    public class JwtOptions
    {
        public TimeSpan JwtExpiration;
        public TimeSpan JwtRememberExpiration;
        public string JwtSecret { get; set; }
        public string JwtIssuer { get; set; }
        public string JwtAudience { get; set; }
    }
}
