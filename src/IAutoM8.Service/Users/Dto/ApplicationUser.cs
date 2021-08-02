using System;
using System.Security.Claims;

namespace IAutoM8.Service.Users.Dto
{
    public class ApplicationUser : ClaimsIdentity
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
    }
}