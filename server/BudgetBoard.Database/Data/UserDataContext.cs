using BudgetBoard.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BudgetBoard.Database.Data;

/// <summary>
/// Represents the Entity Framework Core database context for user-related data and entities in the budgeting application.
/// </summary>
public class UserDataContext(DbContextOptions<UserDataContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    // Constants needed for handling Postgres Large Objects
    // https://github.com/postgres/postgres/blob/master/src/include/libpq/libpq-fs.h
    private const int INV_WRITE = 0x00020000;
    private const int INV_READ = 0x00040000;

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

    // https://www.postgresql.org/docs/current/lo-interfaces.html
    public async Task<long> WriteLargeObjectAsync(long objectId, byte[] data)
    {
        var conn = (NpgsqlConnection)Database.GetDbConnection();

        await conn.OpenAsync();
        using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // 1. Delete the large object if it already exists
            if (objectId != 0)
            {
                using var unlinkCmd = new NpgsqlCommand("SELECT lo_unlink(@oid)", conn, transaction);
                unlinkCmd.Parameters.AddWithValue("oid", objectId);
                await unlinkCmd.ExecuteNonQueryAsync();
            }

            // 2. Create the new large object
            using var createCmd = new NpgsqlCommand("SELECT lo_create(0)", conn, transaction);
            objectId = Convert.ToInt64(await createCmd.ExecuteScalarAsync());

            // 3. Open - Returns a file descriptor (fd) for the specific OID
            using var openCmd = new NpgsqlCommand("SELECT lo_open(@oid, @mode)", conn, transaction);
            openCmd.Parameters.AddWithValue("oid", objectId);
            openCmd.Parameters.AddWithValue("mode", INV_READ | INV_WRITE);
            int fd = (int)await openCmd.ExecuteScalarAsync();

            try
            {
                // 4. Write - Use lo_write(fd, data)
                using var writeCmd = new NpgsqlCommand("SELECT lowrite(@fd, @data)", conn, transaction);
                writeCmd.Parameters.AddWithValue("fd", fd);
                writeCmd.Parameters.AddWithValue("data", data);
                await writeCmd.ExecuteNonQueryAsync();
            }
            finally
            {
                // 5. Close the file descriptor
                using var closeCmd = new NpgsqlCommand("SELECT lo_close(@fd)", conn, transaction);
                closeCmd.Parameters.AddWithValue("fd", fd);
                await closeCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }

        return objectId;
    }

    // https://www.postgresql.org/docs/current/lo-interfaces.html
    public async Task<byte[]> ReadLargeObjectAsync(long objectId)
    {
        var conn = (NpgsqlConnection)Database.GetDbConnection();

        await conn.OpenAsync();
        using var memoryStream = new MemoryStream();

        using var transaction = await conn.BeginTransactionAsync();

        try
        {
            // 1. Open - Returns a file descriptor (fd) for the specific OID
            using var openCmd = new NpgsqlCommand("SELECT lo_open(@oid, @mode)", conn, transaction);
            openCmd.Parameters.AddWithValue("oid", objectId);
            openCmd.Parameters.AddWithValue("mode", INV_READ | INV_WRITE);
            int fd = (int)await openCmd.ExecuteScalarAsync();

            try
            {
                // 2. Read in chunks using lo_read
                int bufferSize = 8192;
                byte[] buffer;
                bool eof = false;

                while (!eof)
                {
                    await using (var readCmd = new NpgsqlCommand("SELECT loread(@fd, @len)", conn, transaction))
                    {
                        readCmd.Parameters.AddWithValue("fd", fd);
                        readCmd.Parameters.AddWithValue("len", bufferSize);

                        buffer = (byte[]) await readCmd.ExecuteScalarAsync();
                        if (buffer == null || buffer.Length == 0)
                        {
                            eof = true;
                        }
                        else
                        {
                            // Process the chunk (e.g., write to a file or stream)
                            await memoryStream.WriteAsync(buffer);
                        }
                    }
                }
            }
            finally
            {
                // 4. Close the file descriptor
                using var closeCmd = new NpgsqlCommand("SELECT lo_close(@fd)", conn, transaction);
                closeCmd.Parameters.AddWithValue("fd", fd);
                await closeCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }

        return memoryStream.ToArray();
    }
}
