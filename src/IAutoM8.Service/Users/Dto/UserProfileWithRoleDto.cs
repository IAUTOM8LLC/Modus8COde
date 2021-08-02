using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Users.Dto
{
    public class UserProfileWithRoleDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool EmailConfirmed { get; set; }
        public int ManagerCount { get; set; }
        public int WorkerCount { get; set; }
        public int TotalCount { get; set; }

    }
}
