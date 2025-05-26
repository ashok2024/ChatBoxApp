using ChatApp.Api.Services;
using ChatApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                    ProfileImageUrl = !string.IsNullOrEmpty(u.ImagePath) ? $"{Request.Scheme}://{Request.Host}/{u.ImagePath}": $"{Request.Scheme}://{Request.Host}/uploads/Sample_User_Icon.png"
                })
                .ToList();

            return Ok(userDtos);
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