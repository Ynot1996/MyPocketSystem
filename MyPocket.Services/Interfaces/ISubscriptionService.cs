using MyPocket.Core.Models;
using MyPocket.Shared.DTOs;

namespace MyPocket.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<IEnumerable<SubscriptionPlan>> GetAllPlansAsync();
        Task<SubscriptionPlan?> GetPlanByIdAsync(Guid planId);
        Task<UserSubscription?> GetActiveSubscriptionAsync(Guid userId);
        Task<bool> SubscribeAsync(Guid userId, Guid planId);
        Task<bool> CancelSubscriptionAsync(Guid userId);
        Task<bool> IsSubscriptionActiveAsync(Guid userId);
    }
}