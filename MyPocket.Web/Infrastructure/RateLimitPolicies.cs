namespace MyPocket.Web.Infrastructure
{
    /// <summary>Names of the rate-limit policies registered in Program.cs.</summary>
    public static class RateLimitPolicies
    {
        /// <summary>Anonymous guest creation, per client IP — stops bots filling the DB.</summary>
        public const string GuestStart = "guest-start";

        /// <summary>Paid AI calls (receipt scan, text parsing, coaching), per user — caps API spend.</summary>
        public const string Ai = "ai";
    }
}
