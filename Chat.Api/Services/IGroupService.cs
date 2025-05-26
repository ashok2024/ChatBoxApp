using ChatApp.API.Dtos;
using ChatApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Api.Services
{
    public interface IGrouupService
    {
        Task<Group> CreateGroup(CreateGroupDto dto, IFormFile? image);
        Task<List<Group>> GetGroupsForUserAsync(string userEmail);
        Task<Group?> GetGroupByIdAsync(int groupId);
        Task<bool> IsUserInGroupAsync(string userEmail, int groupId);
    }
}