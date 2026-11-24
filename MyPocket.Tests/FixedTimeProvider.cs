namespace MyPocket.Tests
{
    /// <summary>A clock the test controls, for date-sensitive logic (daily caps, streaks).</summary>
    public sealed class FixedTimeProvider : TimeProvider
    {
        public DateTimeOffset Now { get; set; }

        public FixedTimeProvider(DateTime utcNow)
        {
            Now = new DateTimeOffset(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
        }

        public override DateTimeOffset GetUtcNow() => Now;

        public void Advance(TimeSpan by) => Now = Now.Add(by);
    }
}
