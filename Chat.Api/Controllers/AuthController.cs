using Microsoft.AspNetCore.Mvc;
using ChatApp.API.Data;
using ChatApp.API.Models;
using ChatApp.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using SmartAttendance.Api.DTOs;
using ChatApp.API.Dtos;

namespace ChatApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext dbContext, JwtService jwtService)
        {
            _dbContext = dbContext;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto userDto)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == userDto.Email))
                return BadRequest("User already exists");

            userDto.Password = HashPassword(userDto.Password);
            User user = new User();
            user.Username = userDto.Email;
            user.PasswordHash = userDto.Password;
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Username == login.Username);
            if (user == null || user.PasswordHash != HashPassword(login.Password))
                return Unauthorized();

            var token = _jwtService.GenerateToken(user.Username);

            return Ok(new { Token = token });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
