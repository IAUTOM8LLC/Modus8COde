using System;
using System.Collections.Generic;
using System.Text;

namespace IAutoM8.Service.Users.Dto
{
    public class UserFilterItemDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public IList<string> Roles { get; set; }
    }
}
