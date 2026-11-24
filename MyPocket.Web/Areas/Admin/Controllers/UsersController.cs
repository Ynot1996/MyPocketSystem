using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;

namespace MyPocket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly MyPocketDBContext _context;

        public UsersController(MyPocketDBContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            return View();
        }

        private static readonly string[] AllowedRoles = { "FreeMember", "PaidMember", "Admin" };

        // Explicit parameters instead of binding the entity: the scaffolded form
        // posted PasswordHash straight from a text box, so admin-created users
        // were stored with a plaintext "hash" and could never log in.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string email, string password, string? nickname, string role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                ModelState.AddModelError(string.Empty, "Email and a password of at least 8 characters are required.");
                return View();
            }
            if (!AllowedRoles.Contains(role))
            {
                ModelState.AddModelError(string.Empty, "Unknown role.");
                return View();
            }
            if (await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted))
            {
                ModelState.AddModelError(string.Empty, "This email is already registered.");
                return View();
            }

            var now = DateTime.UtcNow;
            _context.Users.Add(new MyPocket.Core.Models.User
            {
                UserId = Guid.NewGuid(),
                Email = email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Nickname = string.IsNullOrWhiteSpace(nickname) ? email.Split('@')[0] : nickname.Trim(),
                Role = role,
                CreationDate = now,
                LastLoginDate = now,
                UpdatedAt = now,
                IsDeleted = false
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Loads the existing row and copies only admin-editable fields onto it, so
        // the password hash and system timestamps are never round-tripped through
        // the browser. A new password is applied only when one is supplied.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, string email, string? nickname, string role, bool isDeleted, string? newPassword)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(email) || !AllowedRoles.Contains(role))
            {
                ModelState.AddModelError(string.Empty, "A valid email and role are required.");
                return View(user);
            }
            if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 8)
            {
                ModelState.AddModelError(string.Empty, "The new password must be at least 8 characters.");
                return View(user);
            }
            if (await _context.Users.AnyAsync(u => u.Email == email && u.UserId != id && !u.IsDeleted))
            {
                ModelState.AddModelError(string.Empty, "Another account already uses this email.");
                return View(user);
            }

            user.Email = email.Trim();
            user.Nickname = string.IsNullOrWhiteSpace(nickname) ? user.Nickname : nickname.Trim();
            user.Role = role;
            user.IsDeleted = isDeleted;
            if (!string.IsNullOrEmpty(newPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
