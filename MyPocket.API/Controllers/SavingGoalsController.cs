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
    public class SavingGoalsController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public SavingGoalsController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SavingGoalDTO>>> GetAll()
        {
            var goals = await _context.SavingGoals
                .Where(g => !g.IsDeleted)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<SavingGoalDTO>>(goals));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SavingGoalDTO>> Get(Guid id)
        {
            var goal = await _context.SavingGoals
                .FirstOrDefaultAsync(g => g.GoalId == id && !g.IsDeleted);
            
            if (goal == null) return NotFound();
            return Ok(_mapper.Map<SavingGoalDTO>(goal));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<SavingGoalDTO>>> GetByUser(Guid userId)
        {
            var goals = await _context.SavingGoals
                .Where(g => g.UserId == userId && !g.IsDeleted)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();
            
            return Ok(_mapper.Map<IEnumerable<SavingGoalDTO>>(goals));
        }

        [HttpPost]
        public async Task<ActionResult<SavingGoalDTO>> Create([FromBody] CreateSavingGoalDTO dto)
        {
            var goal = _mapper.Map<SavingGoal>(dto);
            goal.GoalId = Guid.NewGuid();
            goal.CreatedAt = DateTime.UtcNow;
            goal.UpdatedAt = DateTime.UtcNow;
            
            _context.SavingGoals.Add(goal);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(Get), 
                new { id = goal.GoalId }, 
                _mapper.Map<SavingGoalDTO>(goal));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSavingGoalDTO dto)
        {
            var goal = await _context.SavingGoals.FindAsync(id);
            if (goal == null) return NotFound();

            _mapper.Map(dto, goal);
            goal.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var goal = await _context.SavingGoals.FindAsync(id);
            if (goal == null) return NotFound();
            
            goal.IsDeleted = true;
            goal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}