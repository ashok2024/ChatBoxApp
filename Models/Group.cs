namespace ChatApp.API.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public List<string> MembersEmails { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public string? ImagePath { get; set; }
    }
}