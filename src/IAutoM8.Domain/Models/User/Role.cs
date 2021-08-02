using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace IAutoM8.Domain.Models.User
{
    public class Role : IdentityRole<Guid>
    {
        public virtual IList<UserRole> UserRoles { get; set; }
    }
}
