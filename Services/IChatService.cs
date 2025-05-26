using ChatApp.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChatApp.Api.Services
{
    public interface IChatService
    {
        Task SaveMessageAsync(Message message);
        Task<IEnumerable<Message>> GetDirectMessagesAsync(string user1Email, string user2Email);
        Task<IEnumerable<Message>> GetPublicMessagesAsync(); // If you want a public chat as well
        Task<IEnumerable<Message>> GetGroupMessagesAsync(int groupId);

    }
}