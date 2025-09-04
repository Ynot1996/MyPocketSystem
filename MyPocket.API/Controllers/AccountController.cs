using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.DTOs;
using MyPocket.Core.Models;

namespace MyPocket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly MyPocketDBContext _context;

        public AccountController(MyPocketDBContext context)
        {
            _context = context;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required.");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && !u.IsDeleted);
            if (user == null)
                return Unauthorized("Invalid email or password.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password.");

            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Login successful", UserId = user.UserId, Email = user.Email, Nickname = user.Nickname });
        }
    }
}
