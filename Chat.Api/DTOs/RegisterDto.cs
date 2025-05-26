namespace ChatApp.API.Dtos
{
    public class RegisterDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? MobileNo { get; set; }
        public string Role { get; set; } = "User";  // Default role
    }
}
