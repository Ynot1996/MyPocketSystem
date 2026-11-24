using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyPocket.Services.Ai;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.Resources;
using MyPocket.Shared.ViewModels.Capture;
using MyPocket.Web.Infrastructure;
using MyPocket.Web.Services;

namespace MyPocket.Web.Areas.User.Controllers
{
    /// <summary>
    /// JSON endpoints that turn a receipt photo or a sentence (typed or dictated)
    /// into a transaction draft. The draft pre-fills the normal form; nothing is
    /// saved until the user confirms.
    /// </summary>
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    [Route("User/Capture/[action]")]
    public class CaptureController : Controller
    {
        /// <summary>Formats Claude reads; HEIC from iPhones is converted by the browser picker.</summary>
        private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };
        private const long MaxImageBytes = 5 * 1024 * 1024;

        private readonly IReceiptScanner _receipts;
        private readonly ITransactionTextParser _textParser;
        private readonly ICategoryService _categories;
        private readonly ICurrencyService _currency;
        private readonly ILocalizationService _localizer;
        private readonly IAppTelemetry _telemetry;

        public CaptureController(
            IReceiptScanner receipts,
            ITransactionTextParser textParser,
            ICategoryService categories,
            ICurrencyService currency,
            ILocalizationService localizer,
            IAppTelemetry telemetry)
        {
            _receipts = receipts;
            _textParser = textParser;
            _categories = categories;
            _currency = currency;
            _localizer = localizer;
            _telemetry = telemetry;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicies.Ai)]
        [RequestSizeLimit(MaxImageBytes + 64 * 1024)]
        public async Task<IActionResult> Receipt(IFormFile? image, CancellationToken cancellationToken)
        {
            if (!_receipts.IsAvailable)
                return Problem(_localizer.GetString("AiNotConfigured"), statusCode: StatusCodes.Status503ServiceUnavailable);

            if (image == null || image.Length == 0)
                return BadRequest(new { error = _localizer.GetString("ReceiptMissing") });
            if (image.Length > MaxImageBytes)
                return BadRequest(new { error = _localizer.GetString("ReceiptTooLarge") });
            if (!AllowedImageTypes.Contains(image.ContentType))
                return BadRequest(new { error = _localizer.GetString("ReceiptBadType") });

            // Processed in memory only — the photo is never written to disk or the DB.
            await using var stream = new MemoryStream();
            await image.CopyToAsync(stream, cancellationToken);

            var draft = await _receipts.ScanAsync(new AiImage(stream.ToArray(), image.ContentType), _currency.CurrentCode, cancellationToken);
            _telemetry.TrackEvent("ReceiptScanned", new Dictionary<string, string> { ["success"] = (draft != null).ToString() });

            if (draft == null)
                return UnprocessableEntity(new { error = _localizer.GetString("ReceiptUnreadable") });

            return Ok(await ToResponseAsync(draft));
        }

        public record TextRequest(string? Text);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicies.Ai)]
        public async Task<IActionResult> Text([FromBody] TextRequest request, CancellationToken cancellationToken)
        {
            var draft = await _textParser.ParseAsync(request.Text ?? string.Empty, _currency.CurrentCode, cancellationToken);
            _telemetry.TrackEvent("TextParsed", new Dictionary<string, string>
            {
                ["success"] = (draft != null).ToString(),
                ["ai"] = (draft?.UsedAi ?? false).ToString()
            });

            if (draft == null)
                return UnprocessableEntity(new { error = _localizer.GetString("TextUnparsed") });

            return Ok(await ToResponseAsync(draft));
        }

        /// <summary>Adds the user's matching category id so the form dropdown can pre-select it.</summary>
        private async Task<object> ToResponseAsync(CaptureDraft draft)
        {
            Guid? categoryId = null;
            var userId = User.UserIdOrNull();
            if (draft.CategoryKey != null && userId.HasValue)
            {
                var vm = await _categories.GetUserCategoriesAsync(userId.Value);
                categoryId = vm.DefaultExpenseCategories
                    .Concat(vm.DefaultIncomeCategories)
                    .Concat(vm.UserExpenseCategories)
                    .Concat(vm.UserIncomeCategories)
                    .FirstOrDefault(c => c.CategoryName == draft.CategoryKey)?.CategoryId;
            }

            return new
            {
                amount = draft.Amount,
                currency = draft.Currency,
                date = draft.Date?.ToString("yyyy-MM-dd"),
                description = string.Join(" · ", new[] { draft.Merchant, draft.Description }
                    .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct()),
                categoryId,
                categoryLabel = draft.CategoryKey != null ? _localizer.GetString(draft.CategoryKey) : null,
                source = draft.Source,
                usedAi = draft.UsedAi
            };
        }
    }
}
