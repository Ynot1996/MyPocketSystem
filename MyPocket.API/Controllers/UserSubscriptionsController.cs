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
    public class UserSubscriptionsController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public UserSubscriptionsController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSubscriptionDTO>>> GetAll()
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<UserSubscriptionDTO>>(subscriptions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserSubscriptionDTO>> Get(Guid id)
        {
            var subscription = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.SubscriptionId == id);
                
            if (subscription == null) return NotFound();
            return Ok(_mapper.Map<UserSubscriptionDTO>(subscription));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserSubscriptionDTO>>> GetByUser(Guid userId)
        {
            var subscriptions = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
            
            return Ok(_mapper.Map<IEnumerable<UserSubscriptionDTO>>(subscriptions));
        }

        [HttpPost]
        public async Task<ActionResult<UserSubscriptionDTO>> Create([FromBody] CreateUserSubscriptionDTO dto)
        {
            var subscription = _mapper.Map<UserSubscription>(dto);
            subscription.SubscriptionId = Guid.NewGuid();
            
            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            var created = await _context.UserSubscriptions
                .Include(s => s.User)
                .Include(s => s.Plan)
                .FirstAsync(s => s.SubscriptionId == subscription.SubscriptionId);
            
            return CreatedAtAction(nameof(Get), 
                new { id = created.SubscriptionId }, 
                _mapper.Map<UserSubscriptionDTO>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserSubscriptionDTO dto)
        {
            var subscription = await _context.UserSubscriptions.FindAsync(id);
            if (subscription == null) return NotFound();

            _mapper.Map(dto, subscription);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var subscription = await _context.UserSubscriptions.FindAsync(id);
            if (subscription == null) return NotFound();
            
            subscription.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}