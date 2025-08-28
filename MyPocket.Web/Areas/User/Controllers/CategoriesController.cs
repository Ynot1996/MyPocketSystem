using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.ViewModels.Categories;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember")]
    public class CategoriesController : Controller
    {
        private readonly MyPocketDBContext _context;
        private static readonly Guid AdminUserId = Guid.Parse("ef908f96-9557-477d-97f1-4b1d766150bc");

        public CategoriesController(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            var userId = Guid.Parse(userIdString);

            var categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();
            var vm = new UserCategoryViewModel
            {
                DefaultIncomeCategories = categories.Where(c => c.UserId == AdminUserId && c.CategoryType == "收入").ToList(),
                DefaultExpenseCategories = categories.Where(c => c.UserId == AdminUserId && c.CategoryType == "支出").ToList(),
                UserIncomeCategories = categories.Where(c => c.UserId == userId && c.CategoryType == "收入").ToList(),
                UserExpenseCategories = categories.Where(c => c.UserId == userId && c.CategoryType == "支出").ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string newCategoryName, string newCategoryType)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account", new { area = "" });
            var userId = Guid.Parse(userIdString);
            if (!string.IsNullOrWhiteSpace(newCategoryName) && (newCategoryType == "收入" || newCategoryType == "支出"))
            {
                var category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    UserId = userId,
                    CategoryName = newCategoryName.Trim(),
                    CategoryType = newCategoryType,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account", new { area = "" });
            var userId = Guid.Parse(userIdString);
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == userId);
            if (category != null)
            {
                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
