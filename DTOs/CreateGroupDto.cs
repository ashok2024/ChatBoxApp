
namespace ChatApp.API.Dtos
{
    public class CreateGroupDto
    {
        public string GroupName { get; set; }
        public List<string> Members { get; set; }
    }
}