using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace IAutoM8.Infrastructure.JWT
{
    public class ConfirmEmailDataProtectorTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public ConfirmEmailDataProtectorTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<ConfirmEmailDataProtectionTokenProviderOptions> options) : base(dataProtectionProvider, options)
        {
        }
    }
    public class ConfirmEmailDataProtectionTokenProviderOptions : DataProtectionTokenProviderOptions { }
}
