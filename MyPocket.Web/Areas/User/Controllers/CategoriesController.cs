using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Categories;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            var userId = Guid.Parse(userIdString);
            var vm = await _categoryService.GetUserCategoriesAsync(userId);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string newCategoryName, string newCategoryType)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account", new { area = "" });
            var userId = Guid.Parse(userIdString);
            await _categoryService.CreateCategoryAsync(userId, newCategoryName, newCategoryType);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account", new { area = "" });
            var userId = Guid.Parse(userIdString);
            await _categoryService.DeleteCategoryAsync(userId, id);
            return RedirectToAction("Index");
        }
    }
}
