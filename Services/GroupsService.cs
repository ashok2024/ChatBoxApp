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
        private readonly ICloudinaryService _cloudinaryService;

        public GroupService(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Group> CreateGroup(CreateGroupDto dto, IFormFile? image)
        {
            string? imagePath = null;

            if (image != null && image.Length > 0)
            {
                imagePath = await _cloudinaryService.UploadImageAsync(image);
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