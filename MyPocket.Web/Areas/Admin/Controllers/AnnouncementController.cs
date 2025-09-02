using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Core.Models;

namespace MyPocket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AnnouncementController : Controller
    {
        private readonly MyPocketDBContext _context;

        public AnnouncementController(MyPocketDBContext context)
        {
            _context = context;
        }

        // GET: Admin/Announcement
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.Announcements.Include(a => a.Admin).OrderByDescending(a => a.PublishDate).ToListAsync();
            return View(announcements);
        }

        // GET: Admin/Announcement/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var announcement = await _context.Announcements.Include(a => a.Admin).FirstOrDefaultAsync(a => a.AnnouncementId == id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // GET: Admin/Announcement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Announcement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content")] Announcement announcement)
        {
            if (ModelState.IsValid)
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
                if (admin == null)
                {
                    return Unauthorized();
                }
                announcement.AnnouncementId = Guid.NewGuid();
                announcement.AdminId = admin.UserId;
                announcement.PublishDate = DateTime.UtcNow;
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // GET: Admin/Announcement/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Admin/Announcement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("AnnouncementId,Title,Content")] Announcement announcement)
        {
            if (id != announcement.AnnouncementId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Announcements.FindAsync(id);
                    if (existing == null)
                        return NotFound();
                    existing.Title = announcement.Title;
                    existing.Content = announcement.Content;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnouncementExists(announcement.AnnouncementId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // GET: Admin/Announcement/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var announcement = await _context.Announcements.Include(a => a.Admin).FirstOrDefaultAsync(a => a.AnnouncementId == id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Admin/Announcement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AnnouncementExists(Guid id)
        {
            return _context.Announcements.Any(e => e.AnnouncementId == id);
        }
    }
}
