using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.DTOs;
using AutoMapper;

namespace MyPocket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public UsersController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAll()
        {
            var users = await _context.Users.Where(u => !u.IsDeleted).ToListAsync();
            return Ok(_mapper.Map<IEnumerable<UserDTO>>(users));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> Get(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);
            if (user == null) return NotFound();
            return Ok(_mapper.Map<UserDTO>(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserDTO>> Create([FromForm] CreateUserDTO dto)
        {
            var user = _mapper.Map<User>(dto);
            user.UserId = Guid.NewGuid();
            user.Role = "FreeMember";
            user.CreationDate = DateTime.UtcNow;
            user.LastLoginDate = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(Get), 
                new { id = user.UserId }, 
                _mapper.Map<UserDTO>(user));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _mapper.Map(dto, user);
            user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}
