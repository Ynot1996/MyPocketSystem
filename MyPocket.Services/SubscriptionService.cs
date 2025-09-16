using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlan>> GetAllPlansAsync();
        Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId);
        Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId);
        Task<bool> SubscribeAsync(Guid userId, Guid planId);
        Task<bool> CancelSubscriptionAsync(Guid userId);
        Task<bool> IsSubscriptionActiveAsync(Guid userId);
        Task<bool> SubscribeToBasicPlanAsync(Guid userId);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly MyPocketDBContext _context;

        public SubscriptionService(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<List<SubscriptionPlan>> GetAllPlansAsync()
        {
            return await _context.SubscriptionPlans
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId)
        {
            return await _context.SubscriptionPlans.FindAsync(planId);
        }

        public async Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId)
        {
            return await _context.UserSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.UserId == userId && 
                           s.EndDate > DateTime.UtcNow && 
                           s.Status == "Active")
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsSubscriptionActiveAsync(Guid userId)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);
            return subscription != null;
        }

        public async Task<bool> SubscribeAsync(Guid userId, Guid planId)
        {
            var plan = await GetPlanByIdAsync(planId);
            if (plan == null)
                return false;

            // ���o�Τ��e���q�\
            var currentSubscription = await GetActiveSubscriptionAsync(userId);
            
            // �p�G���{���q�\�A�h�����������
            var startDate = currentSubscription?.EndDate ?? DateTime.UtcNow;

            var subscription = new UserSubscription
            {
                SubscriptionId = Guid.NewGuid(),
                UserId = userId,
                PlanId = planId,
                StartDate = startDate,
                EndDate = startDate.AddDays(plan.DurationDays),
                Status = "Active",
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        PaymentAmount = plan.Price,
                        PaymentWay = "�H�Υd",
                        PaymentDate = DateTime.UtcNow,
                        Status = "Completed",
                        TransactionCode = Guid.NewGuid().ToString()
                    }
                }
            };

            await _context.UserSubscriptions.AddAsync(subscription);

            // ��s�Τᨤ�⬰�I�O�|��
            var user = await _context.Users.FindAsync(userId);
            if (user != null && plan.Price > 0) // �I�O���
            {
                user.Role = "PaidMember";
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelSubscriptionAsync(Guid userId)
        {
            var subscription = await GetActiveSubscriptionAsync(userId);
            if (subscription == null)
                return false;

            subscription.EndDate = DateTime.UtcNow;
            subscription.Status = "Cancelled";

            // �N�Τᨤ���^�򥻷|��
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Role = "FreeMember";
                user.UpdatedAt = DateTime.UtcNow;
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SubscribeToBasicPlanAsync(Guid userId)
        {
            try
            {
                // �M��K�O�򥻷|�����
                var basicPlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.Price == 0 && p.PlanName.Contains("�K�O�򥻷|��"));

                if (basicPlan == null)
                    return false;

                // �ˬd�Τ�O�_�w�g�����D���q�\
                var activeSubscription = await GetActiveSubscriptionAsync(userId);
                if (activeSubscription != null)
                    return true; // �w�g���q�\�F�A���ݭn���s�q�\

                // �إ߷s���q�\
                var subscription = new UserSubscription
                {
                    SubscriptionId = Guid.NewGuid(),
                    UserId = userId,
                    PlanId = basicPlan.PlanId,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(basicPlan.DurationDays),
                    Status = "Active"
                };

                await _context.UserSubscriptions.AddAsync(subscription);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}