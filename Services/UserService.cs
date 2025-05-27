using ChatApp.API.Data;
using ChatApp.API.Models;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Api.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ChatApp.Api.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public UserService(AppDbContext context, ICloudinaryService cloudinaryService)
        {

            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == email);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<User?> GetUserByEmailId(string email)
        {
            return await _context.Users.Where(x => x.Username == email).FirstOrDefaultAsync();
        }
        public async Task UpdateUserConnectionIdAsync(string email, string? connectionId)
        {
            var user = await GetUserByEmailAsync(email);
            if (user != null)
            {
                user.CurrentConnectionId = connectionId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ValidateUserCredentials(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            var checkHasPwd = HashPassword(password);
            if (user != null && checkHasPwd == user.PasswordHash)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
        public async Task<User?> RegisterUserAsync(string displayName, string email, string password, string mobileNumber, string role, string ImagePath)
        {
            if (await GetUserByEmailAsync(email) != null)
            {
                return null; // User with this email already exists
            }

            var newUser = new User
            {
                Username = email,
                DisplayName = displayName,
                PasswordHash = HashPassword(password),
                MobileNo = mobileNumber,
                Role = role,
                ImagePath = ImagePath
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }
        public async Task<string> UpdateProfileImageAsync(UpdateProfileRequest model)
        {

            var imagePath = string.Empty;
            try
            {
                // Check if the profile image is provided
                if (!string.IsNullOrEmpty(model.oldURl))
                {

                    imagePath = await _cloudinaryService.UploadImageAsync(model.ProfileImage);

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.email);
                    if (user != null)
                    {
                        user.ImagePath = imagePath;
                        user.DisplayName = model.DisplayName;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to upload image to Cloudinary", ex);
            }

            return imagePath;
        }
    }
}