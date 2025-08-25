using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;

namespace MyPocket.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetsController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        public BudgetsController(MyPocketDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var budgets = await _context.Budgets.ToListAsync();
            return Ok(budgets);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Budget budget)
        {
            budget.BudgetId = Guid.NewGuid();
            budget.CreatedAt = DateTime.UtcNow;
            budget.UpdatedAt = DateTime.UtcNow;
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = budget.BudgetId }, budget);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Budget budget)
        {
            var existing = await _context.Budgets.FindAsync(id);
            if (existing == null) return NotFound();
            existing.Amount = budget.Amount;
            existing.CategoryId = budget.CategoryId;
            existing.BudgetYear = budget.BudgetYear;
            existing.BudgetMonth = budget.BudgetMonth;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.IsDeleted = budget.IsDeleted;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
