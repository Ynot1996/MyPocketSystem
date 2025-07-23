using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models; 

namespace MyPocket.DataAccess.Data
{
    public partial class MyPocketDBContext : DbContext
    {
        public MyPocketDBContext()
        {
        }

        public MyPocketDBContext(DbContextOptions<MyPocketDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Announcement> Announcements { get; set; }
        public virtual DbSet<Budget> Budgets { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<SavingGoal> SavingGoals { get; set; }
        public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Announcement>(entity =>
            {
                entity.Property(e => e.AnnouncementId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.PublishDate).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Admin).WithMany(p => p.Announcements).OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<Budget>(entity =>
            {
                entity.Property(e => e.BudgetId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Category).WithMany(p => p.Budgets).OnDelete(DeleteBehavior.Restrict); 
                entity.HasOne(d => d.User).WithMany(p => p.Budgets).OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.CategoryId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.User).WithMany(p => p.Categories).OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(e => e.PaymentId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.PaymentDate).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Subscription).WithMany(p => p.Payments).OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<SavingGoal>(entity =>
            {
                entity.Property(e => e.GoalId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.User).WithMany(p => p.SavingGoals).OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.Property(e => e.PlanId).HasDefaultValueSql("(newid())");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.TransactionId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.TransactionDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.TransactionType).IsFixedLength();
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
                entity.HasOne(d => d.Category).WithMany(p => p.Transactions).OnDelete(DeleteBehavior.Restrict); 
                entity.HasOne(d => d.User).WithMany(p => p.Transactions).OnDelete(DeleteBehavior.Restrict); 
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
                entity.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.LastLoginDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
            });

            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.Property(e => e.SubscriptionId).HasDefaultValueSql("(newid())");
                entity.HasOne(d => d.Plan).WithMany(p => p.UserSubscriptions).OnDelete(DeleteBehavior.Restrict); 
                entity.HasOne(d => d.User).WithMany(p => p.UserSubscriptions).OnDelete(DeleteBehavior.Restrict); 
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}