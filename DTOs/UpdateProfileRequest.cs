namespace SmartAttendance.Api.DTOs
{
    public class UpdateProfileRequest
    {
        public string? DisplayName { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? oldURl { get; set; }
        public string? email { get; set; }
    }
}