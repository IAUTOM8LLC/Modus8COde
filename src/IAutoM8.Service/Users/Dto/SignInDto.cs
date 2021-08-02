namespace IAutoM8.Service.Users.Dto
{
    public class SignInDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
    }
}
