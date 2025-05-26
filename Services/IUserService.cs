using ChatApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Api.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserConnectionIdAsync(string email, string? connectionId);
        Task<bool> ValidateUserCredentials(string email, string password);
         Task<User?> RegisterUserAsync(string displayName, string email, string password, string mobileNumber, string role,string ImagePath);
    }
}