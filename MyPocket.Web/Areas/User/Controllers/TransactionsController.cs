using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.ViewModels.Transactions;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember")]
    public class TransactionsController : Controller
    {
        private readonly MyPocketDBContext _context;
        
        public TransactionsController(MyPocketDBContext context)
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

            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Admin");

            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => (c.UserId == userId || (adminUser != null && c.UserId == adminUser.UserId)) && !c.IsDeleted)
                .OrderBy(c => c.CategoryType)
                .ThenBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = $"{c.CategoryName}",
                    Group = new SelectListGroup { Name = c.CategoryType }
                })
                .ToListAsync();

            ViewBag.Categories = categories;

            return View(transactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionCreateModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            // 修改 2: 現在 ModelState 會根據 ViewModel 的規則來驗證，不會有 TransactionType 的問題
            if (!ModelState.IsValid)
            {
                // 當驗證失敗時，需要重新準備畫面並返回
                // 注意：ReloadIndexView 可能也需要調整以處理這種情況
                return await ReloadIndexView(userId, null); // 或者傳入一個轉換後的 transaction 物件
            }

            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == model.CategoryId && !c.IsDeleted);

                if (category == null)
                {
                    ModelState.AddModelError("CategoryId", "請選擇有效的類別");
                    return await ReloadIndexView(userId, null);
                }

                // 修改 3: 手動建立 Transaction 物件，並從 ViewModel 和後端邏輯中填入資料
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = model.CategoryId,
                    Amount = model.Amount,
                    TransactionDate = model.TransactionDate,
                    Description = model.Description,
                    TransactionType = category.CategoryType, // 從後端邏輯賦值
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "儲存交易時發生錯誤，請稍後再試。");
                return await ReloadIndexView(userId, null);
            }
        }

        // 輔助方法：重新載入 Index 視圖所需的資料
        // 接受額外的 'transaction' 參數，以便在錯誤時能保留用戶輸入
        private async Task<IActionResult> ReloadIndexView(Guid userId, Transaction? transaction = null)
        {
            // 獲取管理員用戶
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Admin");

            // 重新載入交易記錄
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            // 重新載入類別選項（包括管理員的預設類別）
            var categories = await _context.Categories
                .Where(c => (c.UserId == userId || (adminUser != null && c.UserId == adminUser.UserId)) && !c.IsDeleted)
                .OrderBy(c => c.CategoryType)
                .ThenBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = $"{c.CategoryName}",
                    Group = new SelectListGroup { Name = c.CategoryType }
                })
                .ToListAsync();

            ViewBag.Categories = categories;

            // 返回檢視，傳入交易清單
            return View("Index", transactions);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == id && t.UserId == userId);

            if (transaction != null)
            {
                transaction.IsDeleted = true;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}