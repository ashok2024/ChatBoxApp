using ChatApp.Api.Services;
using ChatApp.API.Data;
using ChatApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAttendance.Api.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChatApp.Api.Controllers
{
    [Authorize] // Require authentication to access user list
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _dbContext;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var currentUserEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userDtos = users
                .Where(u => u.Username != currentUserEmail) // Don't show current user in list
                .Select(u => new UserDto
                {
                    Email = u.Username,
                    DisplayName = u.DisplayName,
                    IsOnline = !string.IsNullOrEmpty(u.CurrentConnectionId),
                    ProfileImageUrl = u.ImagePath
                })
                .ToList();

            return Ok(userDtos);
        }
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfileByUser([FromQuery] string userEmail)
        {
            if (userEmail == null)
                return Unauthorized();

            var users = await _userService.GetUserByEmailAsync(userEmail);
            var userDtos = new UserDto
            {
                Email = users.Username,
                DisplayName = users.DisplayName,
                IsOnline = !string.IsNullOrEmpty(users.CurrentConnectionId),
                ProfileImageUrl = users.ImagePath
            };


            return Ok(userDtos);
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest model)
        {
            var res = await _userService.UpdateProfileImageAsync(model);
            return Ok(res);
        }
    }

    public class UserDto
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsOnline { get; set; }
        public string ProfileImageUrl { get; set; }
    }
}