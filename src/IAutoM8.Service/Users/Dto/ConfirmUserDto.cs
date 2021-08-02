namespace IAutoM8.Service.Users.Dto
{
    public class ConfirmUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Code { get; set; }
    }
}
