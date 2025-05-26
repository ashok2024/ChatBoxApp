using ChatApp.API.Data;
using ChatApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Api.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;

        public ChatService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Message>> GetDirectMessagesAsync(string user1Email, string user2Email)
        {
            return await _context.Messages
                .Where(m => (m.SenderEmail == user1Email && m.ReceiverEmail == user2Email) ||
                            (m.SenderEmail == user2Email && m.ReceiverEmail == user1Email))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetPublicMessagesAsync()
        {
            return await _context.Messages
                .Where(m => m.ReceiverEmail == null) // Messages with no specific receiver are public
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Message>> GetGroupMessagesAsync(int groupId)
        {
            return await _context.Messages
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}