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
    public class BudgetsController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public BudgetsController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetDTO>>> GetAll()
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<BudgetDTO>>(budgets));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BudgetDTO>> Get(Guid id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == id && !b.IsDeleted);
                
            if (budget == null) return NotFound();
            return Ok(_mapper.Map<BudgetDTO>(budget));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BudgetDTO>>> GetByUser(Guid userId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && !b.IsDeleted)
                .OrderByDescending(b => b.BudgetYear)
                .ThenByDescending(b => b.BudgetMonth)
                .ToListAsync();
            
            return Ok(_mapper.Map<IEnumerable<BudgetDTO>>(budgets));
        }

        [HttpPost]
        public async Task<ActionResult<BudgetDTO>> Create([FromBody] CreateBudgetDTO dto)
        {
            var budget = _mapper.Map<Budget>(dto);
            budget.BudgetId = Guid.NewGuid();
            budget.CreatedAt = DateTime.UtcNow;
            budget.UpdatedAt = DateTime.UtcNow;
            
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            
            // 重新查詢以包含 Category 信息
            var created = await _context.Budgets
                .Include(b => b.Category)
                .FirstAsync(b => b.BudgetId == budget.BudgetId);
            
            return CreatedAtAction(nameof(Get), 
                new { id = created.BudgetId }, 
                _mapper.Map<BudgetDTO>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBudgetDTO dto)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            _mapper.Map(dto, budget);
            budget.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();
            
            budget.IsDeleted = true;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}
