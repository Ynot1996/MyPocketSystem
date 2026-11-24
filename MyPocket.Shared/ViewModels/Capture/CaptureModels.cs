namespace MyPocket.Shared.ViewModels.Capture
{
    /// <summary>
    /// A pre-filled transaction produced from a receipt photo or a sentence. It is
    /// only a draft: the user always reviews it in the normal form before saving.
    /// </summary>
    /// <param name="CategoryKey">DB category name (e.g. "餐飲") or null if unknown.</param>
    /// <param name="Source">"receipt" or "text".</param>
    /// <param name="UsedAi">False when the offline rule-based parser produced it.</param>
    public record CaptureDraft(
        decimal? Amount,
        string Currency,
        DateTime? Date,
        string? Merchant,
        string? Description,
        string? CategoryKey,
        string Source,
        bool UsedAi);

    /// <summary>One category's spend, already converted into the display currency.</summary>
    public record CategorySpend(string CategoryKey, decimal Amount);

    /// <summary>Aggregated numbers the coach works from — never raw transactions.</summary>
    public record SpendingSummary(
        string Currency,
        decimal IncomeThisMonth,
        decimal ExpenseThisMonth,
        decimal ExpenseLastMonth,
        IReadOnlyList<CategorySpend> ThisMonth,
        IReadOnlyList<CategorySpend> LastMonth,
        int TransactionsThisMonth);

    /// <summary>
    /// A rule-based insight as a localization key plus arguments, so the conclusion
    /// is shown without exposing how it was computed.
    /// </summary>
    /// <param name="Tone">"good", "warn" or "info" — drives the colour in the UI.</param>
    public record Insight(string Key, IReadOnlyList<string> Args, string Tone, string? CategoryKey = null);

    public record CoachTip(string Title, string Body);

    public record CoachAdvice(string Headline, IReadOnlyList<CoachTip> Tips);
}
