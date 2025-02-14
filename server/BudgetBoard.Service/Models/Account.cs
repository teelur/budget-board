﻿using BudgetBoard.Database.Models;
using System.Text.Json.Serialization;

namespace BudgetBoard.Service.Types;

public interface IAccountCreateRequest
{
    public string? SyncID { get; set; }
    public string Name { get; set; }
    public Guid? InstitutionID { get; set; }
    public string Type { get; set; }
    public string Subtype { get; set; }
    public bool HideTransactions { get; set; }
    public bool HideAccount { get; set; }
}
public class AccountCreateRequest() : IAccountCreateRequest
{
    public string? SyncID { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? InstitutionID { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
    public bool HideTransactions { get; set; } = false;
    public bool HideAccount { get; set; } = false;
}

public interface IAccountUpdateRequest
{
    public Guid ID { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Subtype { get; set; }
    public bool HideTransactions { get; set; }
    public bool HideAccount { get; set; }
}
public class AccountUpdateRequest() : IAccountUpdateRequest
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Subtype { get; set; } = string.Empty;
    public bool HideTransactions { get; set; } = false;
    public bool HideAccount { get; set; } = false;

    public AccountUpdateRequest(Account account) : this()
    {
        ID = account.ID;
        Name = account.Name;
        Type = account.Type;
        Subtype = account.Subtype;
        HideTransactions = account.HideTransactions;
        HideAccount = account.HideAccount;
    }
}

public interface IAccountIndexRequest
{
    public Guid ID { get; set; }
    public int Index { get; set; }
}
public class AccountIndexRequest : IAccountIndexRequest
{
    public Guid ID { get; set; }
    public int Index { get; set; }
}

public interface IAccountResponse
{
    public Guid ID { get; set; }
    public string? SyncID { get; set; }
    public string Name { get; set; }
    public Guid? InstitutionID { get; set; }
    public string Type { get; set; }
    public string Subtype { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? BalanceDate { get; set; }
    public bool HideTransactions { get; set; }
    public bool HideAccount { get; set; }
    public DateTime? Deleted { get; set; }
    public int Index { get; set; }
    public Guid UserID { get; set; }
}
public class AccountResponse : IAccountResponse
{
    public Guid ID { get; set; }
    public string? SyncID { get; set; }
    public string Name { get; set; }
    public Guid? InstitutionID { get; set; }
    public string Type { get; set; }
    public string Subtype { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime? BalanceDate { get; set; }
    public bool HideTransactions { get; set; }
    public bool HideAccount { get; set; }
    public DateTime? Deleted { get; set; }
    public int Index { get; set; }
    public Guid UserID { get; set; }

    [JsonConstructor]
    public AccountResponse()
    {
        ID = Guid.NewGuid();
        SyncID = string.Empty;
        Name = string.Empty;
        InstitutionID = null;
        Type = string.Empty;
        Subtype = string.Empty;
        CurrentBalance = 0.0M;
        BalanceDate = null;
        HideTransactions = false;
        HideAccount = false;
        Deleted = null;
        Index = 0;
        UserID = Guid.NewGuid();
    }

    public AccountResponse(Account account)
    {
        ID = account.ID;
        SyncID = account.SyncID;
        Name = account.Name;
        InstitutionID = account.InstitutionID;
        Type = account.Type;
        Subtype = account.Subtype;
        CurrentBalance = account.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.Amount ?? 0;
        BalanceDate = account.Balances.OrderByDescending(b => b.DateTime).FirstOrDefault()?.DateTime;
        HideTransactions = account.HideTransactions;
        HideAccount = account.HideAccount;
        Deleted = account.Deleted;
        Index = account.Index;
        UserID = account.UserID;
    }
}
