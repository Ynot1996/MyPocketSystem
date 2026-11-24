using MyPocket.Shared.ViewModels.Gamification;

namespace MyPocket.Services.Interfaces
{
    public interface IGamificationService
    {
        /// <summary>Current coin balance (sum of the user's ledger).</summary>
        Task<int> GetBalanceAsync(Guid userId);

        /// <summary>
        /// Grants coins for logging a transaction, applying the daily cap and the
        /// once-per-day streak bonus. <paramref name="autoCaptured"/> is true when the
        /// entry came from a receipt scan or voice input (worth a small bonus).
        /// </summary>
        Task<CoinAward> AwardForTransactionAsync(Guid userId, bool autoCaptured);

        /// <summary>Consecutive days (ending today or yesterday) with a logged transaction.</summary>
        Task<int> GetStreakAsync(Guid userId);

        /// <summary>Grants the one-time welcome bonus if the user has never received it.</summary>
        Task EnsureWelcomeBonusAsync(Guid userId);

        /// <summary>The user's city: one building per category plus owned shop items.</summary>
        Task<CityViewModel> GetCityAsync(Guid userId);
    }
}
