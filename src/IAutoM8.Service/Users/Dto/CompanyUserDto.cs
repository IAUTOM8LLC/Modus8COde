using System;
using System.Collections.Generic;

namespace IAutoM8.Service.Users.Dto
{
    public class CompanyUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
        public bool IsLocked { get; set; }
    }
}
