using BudgetBoard.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetBoard.Database.Data;

/// <summary>
/// Represents the Entity Framework Core database context for user-related data and entities in the budgeting application.
/// </summary>
public class UserDataContext(DbContextOptions<UserDataContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
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
    public DbSet<Asset> Assets { get; set; }
    public DbSet<Value> Values { get; set; }
    public DbSet<WidgetSettings> WidgetSettings { get; set; }
    public DbSet<SimpleFinOrganization> SimpleFinOrganizations { get; set; }
    public DbSet<SimpleFinAccount> SimpleFinAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(u =>
        {
            u.HasMany(e => e.Accounts)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserID)
                .IsRequired();

            u.HasMany(e => e.Budgets)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserID)
                .IsRequired();

            u.HasMany(e => e.Goals).WithOne(e => e.User).HasForeignKey(e => e.UserID).IsRequired();
            u.HasMany(e => e.TransactionCategories)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserID);

            u.HasOne(e => e.UserSettings)
                .WithOne(e => e.User)
                .HasForeignKey<UserSettings>(e => e.UserID)
                .IsRequired();

            u.HasMany(e => e.AutomaticRules)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserID)
                .IsRequired();

            u.HasMany(e => e.Assets).WithOne(e => e.User).HasForeignKey(e => e.UserID).IsRequired();

            u.HasMany(e => e.WidgetSettings)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserID)
                .IsRequired();

            u.ToTable("User");
        });

        modelBuilder.Entity<Account>(
            (a) =>
            {
                a.HasMany(e => e.Transactions)
                    .WithOne(e => e.Account)
                    .HasForeignKey(e => e.AccountID);

                a.HasMany(e => e.Balances).WithOne(e => e.Account).HasForeignKey(e => e.AccountID);

                a.ToTable("Account");
            }
        );

        modelBuilder.Entity<Transaction>().ToTable("Transaction");

        modelBuilder.Entity<Budget>().ToTable("Budget");

        modelBuilder.Entity<Goal>(
            (g) =>
            {
                g.HasMany(e => e.Accounts).WithMany(e => e.Goals);

                g.ToTable("Goal");
            }
        );

        modelBuilder.Entity<Balance>().ToTable("Balance");

        modelBuilder.Entity<Category>().ToTable("TransactionCategory");

        modelBuilder.Entity<Institution>(
            (i) =>
            {
                i.HasMany(e => e.Accounts)
                    .WithOne(e => e.Institution)
                    .HasForeignKey(e => e.InstitutionID);

                i.ToTable("Institution");
            }
        );

        modelBuilder.Entity<UserSettings>().ToTable("UserSettings");

        modelBuilder.Entity<AutomaticRule>(r =>
        {
            r.ToTable("AutomaticRule");
        });

        modelBuilder.Entity<RuleParameterBase>(p =>
        {
            p.ToTable("RuleParameter");
            p.HasDiscriminator<string>("ParameterKind")
                .HasValue<RuleCondition>("Condition")
                .HasValue<RuleAction>("Action");
        });

        modelBuilder.Entity<RuleCondition>(c =>
        {
            c.HasOne(e => e.Rule)
                .WithMany(r => r.Conditions)
                .HasForeignKey(e => e.RuleID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RuleAction>(a =>
        {
            a.HasOne(e => e.Rule)
                .WithMany(r => r.Actions)
                .HasForeignKey(e => e.RuleID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Asset>(a =>
        {
            a.HasMany(e => e.Values).WithOne(e => e.Asset).HasForeignKey(e => e.AssetID);

            a.ToTable("Asset");
        });

        modelBuilder.Entity<Value>().ToTable("Value");

        modelBuilder.Entity<WidgetSettings>(w =>
        {
            w.Property(e => e.Configuration).HasColumnType("jsonb");
            w.ToTable("WidgetSettings");
        });

        modelBuilder.Entity<SimpleFinOrganization>().ToTable("SimpleFinOrganization");

        modelBuilder.Entity<SimpleFinAccount>(a =>
        {
            a.HasOne(e => e.Organization)
                .WithMany(e => e.Accounts)
                .HasForeignKey(e => e.OrganizationId);

            a.HasOne(e => e.LinkedAccount)
                .WithOne(e => e.SimpleFinAccount)
                .HasForeignKey<SimpleFinAccount>(e => e.LinkedAccountId);

            a.ToTable("SimpleFinAccount");
        });

        modelBuilder.UseIdentityColumns();
    }
}
