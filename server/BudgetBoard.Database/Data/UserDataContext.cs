using BudgetBoard.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetBoard.Database.Data
{
    public class UserDataContext(DbContextOptions<UserDataContext> options)
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(u =>
            {
                u.HasMany(e => e.Accounts).WithOne(e => e.User).HasForeignKey(e => e.UserID);

                u.HasMany(e => e.Budgets).WithOne(e => e.User).HasForeignKey(e => e.UserID);

                u.HasMany(e => e.Goals).WithOne(e => e.User).HasForeignKey(e => e.UserID);

                u.HasMany(e => e.TransactionCategories)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserID);

                u.HasOne(e => e.UserSettings)
                    .WithOne(e => e.User)
                    .HasForeignKey<UserSettings>(e => e.UserID)
                    .IsRequired();

                u.HasMany(e => e.AutomaticRules).WithOne(e => e.User).HasForeignKey(e => e.UserID);

                u.ToTable("User");
            });

            modelBuilder.Entity<Account>(
                (a) =>
                {
                    a.HasMany(e => e.Transactions)
                        .WithOne(e => e.Account)
                        .HasForeignKey(e => e.AccountID);

                    a.HasMany(e => e.Balances)
                        .WithOne(e => e.Account)
                        .HasForeignKey(e => e.AccountID);

                    a.ToTable("Account");
                }
            );

            modelBuilder.Entity<Institution>(
                (i) =>
                {
                    i.HasMany(e => e.Accounts)
                        .WithOne(e => e.Institution)
                        .HasForeignKey(e => e.InstitutionID);

                    i.ToTable("Institution");
                }
            );

            modelBuilder.Entity<Goal>(
                (g) =>
                {
                    g.HasMany(e => e.Accounts).WithMany(e => e.Goals);

                    g.ToTable("Goal");
                }
            );

            modelBuilder.Entity<Transaction>().ToTable("Transaction");

            modelBuilder.Entity<Budget>().ToTable("Budget");

            modelBuilder.Entity<Balance>().ToTable("Balance");

            modelBuilder.Entity<Category>().ToTable("TransactionCategory");

            modelBuilder.Entity<UserSettings>().ToTable("UserSettings");

            // Base RuleParameter mapping (TPH)
            modelBuilder.Entity<RuleParameterBase>(p =>
            {
                p.ToTable("RuleParameter");
                p.HasDiscriminator<string>("ParameterKind")
                    .HasValue<RuleCondition>("Condition")
                    .HasValue<RuleAction>("Action");
            });

            // Conditions relationship
            modelBuilder.Entity<RuleCondition>(c =>
            {
                c.HasOne(e => e.Rule)
                    .WithMany(r => r.Conditions)
                    .HasForeignKey(e => e.RuleID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Actions relationship
            modelBuilder.Entity<RuleAction>(a =>
            {
                a.HasOne(e => e.Rule)
                    .WithMany(r => r.Actions)
                    .HasForeignKey(e => e.RuleID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<AutomaticRule>(r =>
            {
                r.ToTable("AutomaticRule");
            });

            modelBuilder.UseIdentityColumns();
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Balance> Balances { get; set; }
        public DbSet<Category> TransactionCategories { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<AutomaticRule> AutomaticRules { get; set; }
        public DbSet<RuleParameterBase> RuleParameters { get; set; }
        public DbSet<RuleCondition> RuleConditions { get; set; }
        public DbSet<RuleAction> RuleActions { get; set; }
    }
}
