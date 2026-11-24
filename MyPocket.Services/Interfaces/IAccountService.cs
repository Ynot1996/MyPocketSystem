using MyPocket.Core.Models;

namespace MyPocket.Services.Interfaces
{
    public enum GuestUpgradeResult
    {
        Upgraded,
        NotAGuest,
        EmailTaken
    }

    /// <summary>
    /// Account lifecycle beyond plain login: anonymous guests, upgrading a guest
    /// into a real account, and linking an external (Google) identity.
    /// </summary>
    public interface IAccountService
    {
        /// <summary>Creates a guest account pre-filled with a realistic multi-currency demo.</summary>
        Task<User> CreateGuestAsync();

        /// <summary>Turns a guest into a registered account in place; all data is kept.</summary>
        Task<GuestUpgradeResult> UpgradeGuestAsync(Guid guestId, string email, string password, string? nickname);

        /// <summary>
        /// Finds or creates the local account for a Google sign-in. If the browser
        /// was in guest mode, the guest is either upgraded (new email) or its data
        /// is merged into the existing account (email already registered).
        /// </summary>
        Task<User> ResolveExternalLoginAsync(string email, string? displayName, Guid? currentGuestId);

        /// <summary>Moves a guest's data into <paramref name="targetUserId"/> and closes the guest.</summary>
        Task MergeGuestIntoAsync(Guid guestId, Guid targetUserId);

        /// <summary>Soft-deletes guests created longer ago than <paramref name="olderThan"/>. Returns the count.</summary>
        Task<int> CleanupStaleGuestsAsync(TimeSpan olderThan);
    }
}
