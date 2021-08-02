using System;
using IAutoM8.Global.Enums;

namespace IAutoM8.Service.Users.Dto
{
    public class UserProfileDto
    {
        public string FullName { get; set; }
        public DateTime? Dob { get; set; }
        public GenderEnum Gender { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string ProfileImage { get; set; }
        public string PayoneerEmail { get; set; }
        public int DailyAvailability { get; set; }
    }
}
