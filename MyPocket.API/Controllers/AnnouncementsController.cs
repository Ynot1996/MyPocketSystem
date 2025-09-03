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
    public class AnnouncementsController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public AnnouncementsController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnnouncementDTO>>> GetAll()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Admin)
                .OrderByDescending(a => a.PublishDate)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<AnnouncementDTO>>(announcements));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AnnouncementDTO>> Get(Guid id)
        {
            var announcement = await _context.Announcements
                .Include(a => a.Admin)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);
            
            if (announcement == null) return NotFound();
            return Ok(_mapper.Map<AnnouncementDTO>(announcement));
        }

        [HttpPost]
        public async Task<ActionResult<AnnouncementDTO>> Create([FromBody] CreateAnnouncementDTO dto)
        {
            var announcement = _mapper.Map<Announcement>(dto);
            announcement.AnnouncementId = Guid.NewGuid();
            announcement.PublishDate = DateTime.UtcNow;
            
            _context.Announcements.Add(announcement);
            await _context.SaveChangesAsync();

            var created = await _context.Announcements
                .Include(a => a.Admin)
                .FirstAsync(a => a.AnnouncementId == announcement.AnnouncementId);
            
            return CreatedAtAction(nameof(Get), 
                new { id = created.AnnouncementId }, 
                _mapper.Map<AnnouncementDTO>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnnouncementDTO dto)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();

            _mapper.Map(dto, announcement);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null) return NotFound();
            
            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}