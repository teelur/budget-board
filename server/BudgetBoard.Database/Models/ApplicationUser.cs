﻿using Microsoft.AspNetCore.Identity;

namespace BudgetBoard.Database.Models;
public class ApplicationUser : IdentityUser<Guid>
{
    public string Uid { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public DateTime LastSync { get; set; } = DateTime.MinValue;
    public ICollection<Account> Accounts { get; set; } = [];
    public ICollection<Budget> Budgets { get; set; } = [];
    public ICollection<Goal> Goals { get; set; } = [];
    public ICollection<Category> TransactionCategories { get; set; } = [];
    public ICollection<Institution> Institutions { get; set; } = [];
}