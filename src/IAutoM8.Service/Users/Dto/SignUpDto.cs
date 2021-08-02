namespace IAutoM8.Service.Users.Dto
{
    public class SignUpDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public int OrderId { get; set; }
        public string CompanyWorkerOwnerId { get; set; }
    }
}
