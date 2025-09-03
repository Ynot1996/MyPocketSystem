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
    public class TransactionsController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public TransactionsController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetAll()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => !t.IsDeleted)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<TransactionDTO>>(transactions));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDTO>> Get(Guid id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == id && !t.IsDeleted);
            
            if (transaction == null) return NotFound();
            return Ok(_mapper.Map<TransactionDTO>(transaction));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetByUser(Guid userId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
            
            return Ok(_mapper.Map<IEnumerable<TransactionDTO>>(transactions));
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDTO>> Create([FromBody] CreateTransactionDTO dto)
        {
            var transaction = _mapper.Map<Transaction>(dto);
            transaction.TransactionId = Guid.NewGuid();
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            
            // 重新查詢以包含 Category 信息
            var created = await _context.Transactions
                .Include(t => t.Category)
                .FirstAsync(t => t.TransactionId == transaction.TransactionId);
            
            return CreatedAtAction(nameof(Get), 
                new { id = created.TransactionId }, 
                _mapper.Map<TransactionDTO>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTransactionDTO dto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            _mapper.Map(dto, transaction);
            transaction.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();
            
            transaction.IsDeleted = true;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}
