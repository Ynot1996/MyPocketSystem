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
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public SubscriptionPlansController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDTO>>> GetAll()
        {
            var plans = await _context.SubscriptionPlans.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<SubscriptionPlanDTO>>(plans));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionPlanDTO>> Get(Guid id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound();
            return Ok(_mapper.Map<SubscriptionPlanDTO>(plan));
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionPlanDTO>> Create([FromBody] CreateSubscriptionPlanDTO dto)
        {
            var plan = _mapper.Map<SubscriptionPlan>(dto);
            plan.PlanId = Guid.NewGuid();
            
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(Get), 
                new { id = plan.PlanId }, 
                _mapper.Map<SubscriptionPlanDTO>(plan));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionPlanDTO dto)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound();

            _mapper.Map(dto, plan);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound();
            
            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}