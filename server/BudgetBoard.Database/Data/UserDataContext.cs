using BudgetBoard.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetBoard.Database.Data
{
    public class UserDataContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public UserDataContext(DbContextOptions<UserDataContext> options)
            : base(options) { }

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
                    .HasForeignKey<UserSettings>(e => e.UserID);

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
    }
}
