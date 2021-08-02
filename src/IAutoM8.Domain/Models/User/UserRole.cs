using Microsoft.AspNetCore.Identity;
using System;

namespace IAutoM8.Domain.Models.User
{
    public class UserRole: IdentityUserRole<Guid>
    {
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }
}
