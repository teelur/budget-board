namespace BudgetBoard.Service.Interfaces;

/// <summary>
/// Service responsible for resetting the database and seeding demo data.
/// Intended for demo environments only — enabled via DEMO_RESET_ENABLED=true.
/// </summary>
public interface IDemoSeedService
{
    /// <summary>
    /// Deletes all existing users, then creates a fresh demo user with realistic seed data.
    /// </summary>
    Task ResetAndSeedAsync();
}
