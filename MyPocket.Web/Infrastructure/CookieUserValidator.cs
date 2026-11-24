using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;

namespace MyPocket.Web.Infrastructure
{
    /// <summary>
    /// Re-checks that the account behind an auth cookie still exists. Without this a
    /// cookie outlives its user: a guest cleaned up after 7 days (cookie lasts 14), or a
    /// user an admin deleted, would keep browsing as a ghost. The check runs at most once
    /// per <see cref="RecheckInterval"/> per cookie, so it costs one indexed lookup every
    /// few minutes rather than one per request.
    /// </summary>
    public static class CookieUserValidator
    {
        public static readonly TimeSpan RecheckInterval = TimeSpan.FromMinutes(5);
        private const string CheckedAtKey = "mp.userCheckedAt";

        public static async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var now = DateTimeOffset.UtcNow;
            if (context.Properties.Items.TryGetValue(CheckedAtKey, out var raw)
                && DateTimeOffset.TryParse(raw, out var checkedAt)
                && now - checkedAt < RecheckInterval)
            {
                return;
            }

            var userId = context.Principal?.UserIdOrNull();
            var db = context.HttpContext.RequestServices.GetRequiredService<MyPocketDBContext>();
            var exists = userId != null
                && await db.Users.AnyAsync(u => u.UserId == userId.Value && !u.IsDeleted);

            if (!exists)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            context.Properties.Items[CheckedAtKey] = now.ToString("O");
            context.ShouldRenew = true;
        }
    }
}
