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
    public class PaymentsController : ControllerBase
    {
        private readonly MyPocketDBContext _context;
        private readonly IMapper _mapper;

        public PaymentsController(MyPocketDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetAll()
        {
            var payments = await _context.Payments
                .Include(p => p.Subscription)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return Ok(_mapper.Map<IEnumerable<PaymentDTO>>(payments));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDTO>> Get(Guid id)
        {
            var payment = await _context.Payments
                .Include(p => p.Subscription)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
                
            if (payment == null) return NotFound();
            return Ok(_mapper.Map<PaymentDTO>(payment));
        }

        [HttpGet("subscription/{subscriptionId}")]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetBySubscription(Guid subscriptionId)
        {
            var payments = await _context.Payments
                .Include(p => p.Subscription)
                .Where(p => p.SubscriptionId == subscriptionId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            
            return Ok(_mapper.Map<IEnumerable<PaymentDTO>>(payments));
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDTO>> Create([FromBody] CreatePaymentDTO dto)
        {
            var payment = _mapper.Map<Payment>(dto);
            payment.PaymentId = Guid.NewGuid();
            
            if (payment.PaymentDate == default)
            {
                payment.PaymentDate = DateTime.UtcNow;
            }
            
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var created = await _context.Payments
                .Include(p => p.Subscription)
                .FirstAsync(p => p.PaymentId == payment.PaymentId);
            
            return CreatedAtAction(nameof(Get), 
                new { id = created.PaymentId }, 
                _mapper.Map<PaymentDTO>(created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentDTO dto)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();

            _mapper.Map(dto, payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null) return NotFound();
            
            payment.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}