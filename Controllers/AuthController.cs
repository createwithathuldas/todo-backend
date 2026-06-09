using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Todo_backend.Data;
using Todo_backend.DTOS;
using Todo_backend.Models;
using Todo_backend.Services;

namespace Todo_backend.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;
        public AuthController(ApplicationDbContext context, JwtService JwtService)
        {
            _context = context;
            _jwtService = JwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { Message = "An account with this email already exists." });
            }
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Token = token, User = new { user.Id, user.Name, user.Email } });
        }

    }
}