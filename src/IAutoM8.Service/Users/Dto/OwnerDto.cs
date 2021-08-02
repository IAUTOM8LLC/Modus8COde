
using System;

namespace IAutoM8.Service.Users.Dto
{
    public class OwnerDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public bool IsLocked { get; set; }
    }
}
