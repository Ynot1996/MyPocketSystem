using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using MyPocket.Core.Models;

namespace MyPocket.Web.Infrastructure
{
    /// <summary>
    /// The single place that turns a User row into the claims the app relies on,
    /// so password login, Google login and guest sessions can't drift apart.
    /// </summary>
    public static class UserClaims
    {
        public const string Nickname = "Nickname";
        public const string IsGuest = "IsGuest";

        public static ClaimsPrincipal Build(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.Email),
                new(Nickname, user.Nickname ?? user.Email),
                new(ClaimTypes.Role, user.Role),
                new(IsGuest, user.IsGuest ? "true" : "false")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        public static Task SignInAsync(this HttpContext http, User user) =>
            http.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                Build(user),
                new AuthenticationProperties { IsPersistent = true });

        public static bool IsGuestUser(this ClaimsPrincipal principal) =>
            principal.Identity?.IsAuthenticated == true && principal.FindFirstValue(IsGuest) == "true";

        public static Guid? UserIdOrNull(this ClaimsPrincipal principal) =>
            Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    }
}
