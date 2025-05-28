﻿namespace BudgetBoard.Database.Models;

public enum Currency
{
    USD, // US Dollar
    EUR, // Euro
    GBP, // British Pound
    JPY, // Japanese Yen
    CNY, // Chinese Yuan
    INR, // Indian Rupee
    AUD, // Australian Dollar
    CAD, // Canadian Dollar
    CHF, // Swiss Franc
    SEK, // Swedish Krona
    NZD, // New Zealand Dollar
}

public class UserSettings()
{
    public Guid ID { get; set; }
    public Currency Currency { get; set; } = Currency.USD;
    public Guid UserID { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
