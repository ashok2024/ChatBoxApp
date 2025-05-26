namespace ChatApp.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? CurrentConnectionId { get; set; }
        public string? DisplayName { get; set; }
        public string? MobileNo { get; set; }
        public string? Role { get; set; }
        public string? ImagePath { get; set; }
        // Other user properties like email etc.
    }
}