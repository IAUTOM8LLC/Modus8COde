using IAutoM8.Domain.Models.User;
using IAutoM8.Global;
using IAutoM8.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAutoM8.Service.Infrastructure
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        private readonly IRepo _repo;

        public UserClaimsPrincipalFactory(UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IOptions<IdentityOptions> optionsAccessor,
            IRepo repo)
            : base(userManager, roleManager, optionsAccessor)
        {
            _repo = repo;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(User user)
        {
            var principal = await base.CreateAsync(user);
            var profile = await _repo.Read<UserProfile>().FirstOrDefaultAsync(w => w.UserId == user.Id);
            var primaryUser = user.OwnerId ?? user.Id;

            var claims = (ClaimsIdentity)principal.Identity;
            claims.AddClaims(new List<Claim>
            {
                new Claim(CustomClaimType.fullName, profile.FullName ?? user.UserName),
                new Claim(ClaimTypes.PrimarySid, primaryUser.ToString()),
                new Claim(ClaimTypes.Sid, user.Id.ToString())
            });

            return principal;
        }
    }
}
