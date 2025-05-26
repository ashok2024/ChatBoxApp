using ChatApp.API.Data;
using ChatApp.API.Dtos;
using ChatApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.Api.Services
{
    public class GroupService : IGrouupService
    {
        private readonly AppDbContext _context;

        public GroupService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Group> CreateGroup(CreateGroupDto dto, IFormFile? image)
        {
            string? imagePath = null;

            if (image != null && image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imagePath = $"uploads/{uniqueFileName}";
            }

            //List<string> members = JsonSerializer.Serialize(dto.Members.s);

            var group = new Group
            {
                GroupName = dto.GroupName,
                MembersEmails = dto.Members,
                CreatedAt = DateTime.UtcNow,
                ImagePath = imagePath
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            return await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<List<Group>> GetGroupsForUserAsync(string userEmail)
        {
            return await _context.Groups
                         .Where(g => g.MembersEmails.Contains(userEmail))
                         .ToListAsync();
        }
       
        public async Task<bool> IsUserInGroupAsync(string userEmail, int groupId)
        {
            return await _context.Groups
                .AnyAsync(g => g.Id == groupId && g.MembersEmails.Contains(userEmail));
        }
    }
}