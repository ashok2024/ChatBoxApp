using ChatApp.API.Models;
using ChatApp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ChatApp.API.Dtos;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AccountController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var isValid = await _userService.ValidateUserCredentials(request.Email, request.Password);
            if (!isValid)
            {
                return Unauthorized("Invalid credentials.");
            }

            var user = await _userService.GetUserByEmailAsync(request.Email);
            if (user == null) return Unauthorized("User not found after validation.");

            var token = GenerateJwtToken(user);
            var imageUrl = string.IsNullOrEmpty(user.ImagePath)
                            ? null
                            : $"{Request.Scheme}://{Request.Host}:/{user.ImagePath}";
            return Ok(new { Token = token, user.Username, user.DisplayName , imageUrl });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Name, user.DisplayName)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(2), // Token expiration
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto request, IFormFile? image)
        {
            string? imagePath = null;

            if (image != null && image.Length > 0)
            {
                var cloudinaryService = new CloudinaryService();
                 imagePath = await cloudinaryService.UploadImageAsync(image);
            }

            var newUser = await _userService.RegisterUserAsync(
                request.Name,
                request.Email,
                request.Password,
                request.MobileNo,
                request.Role,
                imagePath
            );

            if (newUser == null)
            {
                return Conflict("User with this email already exists.");
            }

            return Ok(new { Message = "Registration successful!", newUser.Username, newUser.DisplayName });
        }

    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}