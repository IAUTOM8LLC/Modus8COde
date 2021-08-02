namespace IAutoM8.Service.Users.Dto
{
    public class ProfileDto
    {
        public BusinessProfileDto BusinessProfile { get; set; }
        public UserProfileDto UserProfile { get; set; }
        public bool IsVendor { get; set; }
    }
}
