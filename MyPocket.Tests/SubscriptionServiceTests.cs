using MyPocket.Services;

namespace MyPocket.Tests
{
    public class SubscriptionServiceTests
    {
        [Fact]
        public async Task Subscribe_CreatesActiveSubscription_AndPromotesPaidMember()
        {
            using var context = TestHelper.NewContext();
            var plan = TestHelper.SeedPlan(context, "Premium Member (Monthly)", 89m, 30);
            var user = TestHelper.SeedUser(context, role: "FreeMember");
            var service = new SubscriptionService(context);

            var ok = await service.SubscribeAsync(user.UserId, plan.PlanId);

            Assert.True(ok);
            var sub = Assert.Single(context.UserSubscriptions);
            Assert.Equal(user.UserId, sub.UserId);
            Assert.Equal("Active", sub.Status);
            Assert.True(sub.EndDate > DateTime.UtcNow);
            Assert.Equal("PaidMember", context.Users.Single(u => u.UserId == user.UserId).Role);
        }

        [Fact]
        public async Task Subscribe_FreePlan_DoesNotChangeRole()
        {
            using var context = TestHelper.NewContext();
            var plan = TestHelper.SeedPlan(context, "免費基本會員", 0m, 36500);
            var user = TestHelper.SeedUser(context, role: "FreeMember");
            var service = new SubscriptionService(context);

            await service.SubscribeAsync(user.UserId, plan.PlanId);

            Assert.Equal("FreeMember", context.Users.Single(u => u.UserId == user.UserId).Role);
        }

        [Fact]
        public async Task Subscribe_UnknownPlan_ReturnsFalse()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new SubscriptionService(context);

            var ok = await service.SubscribeAsync(user.UserId, Guid.NewGuid());

            Assert.False(ok);
            Assert.Empty(context.UserSubscriptions);
        }

        [Fact]
        public async Task Cancel_EndsActiveSubscription_AndRevertsToFreeMember()
        {
            using var context = TestHelper.NewContext();
            var plan = TestHelper.SeedPlan(context, "Premium Member (Monthly)", 89m, 30);
            var user = TestHelper.SeedUser(context, role: "FreeMember");
            var service = new SubscriptionService(context);

            await service.SubscribeAsync(user.UserId, plan.PlanId);
            Assert.Equal("PaidMember", context.Users.Single().Role);

            var ok = await service.CancelSubscriptionAsync(user.UserId);

            Assert.True(ok);
            var sub = context.UserSubscriptions.Single();
            Assert.Equal("Cancelled", sub.Status);
            Assert.Equal("FreeMember", context.Users.Single().Role);
            Assert.False(await service.IsSubscriptionActiveAsync(user.UserId));
        }

        [Fact]
        public async Task GetActiveSubscription_ExcludesExpiredAndCancelled()
        {
            using var context = TestHelper.NewContext();
            var plan = TestHelper.SeedPlan(context, "Premium", 89m, 30);
            var user = TestHelper.SeedUser(context);
            var service = new SubscriptionService(context);

            // Expired subscription
            context.UserSubscriptions.Add(new Core.Models.UserSubscription
            {
                SubscriptionId = Guid.NewGuid(),
                UserId = user.UserId,
                PlanId = plan.PlanId,
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(-1),
                Status = "Active"
            });
            context.SaveChanges();

            var active = await service.GetActiveSubscriptionAsync(user.UserId);
            Assert.Null(active);
            Assert.False(await service.IsSubscriptionActiveAsync(user.UserId));
        }

        [Fact]
        public async Task GetAllPlans_ReturnsPlansOrderedByPrice()
        {
            using var context = TestHelper.NewContext();
            TestHelper.SeedPlan(context, "Premium Yearly", 799m, 365);
            TestHelper.SeedPlan(context, "Free", 0m, 36500);
            TestHelper.SeedPlan(context, "Premium Monthly", 89m, 30);
            var service = new SubscriptionService(context);

            var plans = (await service.GetAllPlansAsync()).ToList();

            Assert.Equal(3, plans.Count);
            Assert.Equal(0m, plans[0].Price);
            Assert.Equal(89m, plans[1].Price);
            Assert.Equal(799m, plans[2].Price);
        }
    }
}
