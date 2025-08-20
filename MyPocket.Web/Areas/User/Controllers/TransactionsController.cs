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

            // �ק� 2: �{�b ModelState �|�ھ� ViewModel ���W�h�����ҡA���|�� TransactionType �����D
            if (!ModelState.IsValid)
            {
                // �����ҥ��ѮɡA�ݭn���s�ǳƵe���ê�^
                // �`�N�GReloadIndexView �i��]�ݭn�վ�H�B�z�o�ر��p
                return await ReloadIndexView(userId, null); // �Ϊ̶ǤJ�@���ഫ�᪺ transaction ����
            }

            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == model.CategoryId && !c.IsDeleted);

                if (category == null)
                {
                    ModelState.AddModelError("CategoryId", "�п�ܦ��Ī����O");
                    return await ReloadIndexView(userId, null);
                }

                // �ק� 3: ��ʫإ� Transaction ����A�ñq ViewModel �M����޿褤��J���
                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = model.CategoryId,
                    Amount = model.Amount,
                    TransactionDate = model.TransactionDate,
                    Description = model.Description,
                    TransactionType = category.CategoryType, // �q����޿���
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
                ModelState.AddModelError("", "�x�s����ɵo�Ϳ��~�A�еy��A�աC");
                return await ReloadIndexView(userId, null);
            }
        }

        // ���U��k�G���s���J Index ���ϩһݪ����
        // �����B�~�� 'transaction' �ѼơA�H�K�b���~�ɯ�O�d�Τ��J
        private async Task<IActionResult> ReloadIndexView(Guid userId, Transaction? transaction = null)
        {
            // ����޲z���Τ�
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Admin");

            // ���s���J����O��
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();

            // ���s���J���O�ﶵ�]�]�A�޲z�����w�]���O�^
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

            // ��^�˵��A�ǤJ����M��
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