using System.ComponentModel.DataAnnotations;

namespace ChatApp.API.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string? SenderEmail { get; set; }
        public string? SenderDisplayName { get; set; }

        public string? ReceiverEmail { get; set; }

        [Required]
        public string? Text { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
         public int? GroupId { get; set; }
    }
}
