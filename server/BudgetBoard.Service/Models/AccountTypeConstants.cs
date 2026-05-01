namespace BudgetBoard.Service.Models;

public static class AccountTypeConstants
{
    public static readonly IEnumerable<IAccountType> DefaultAccountTypes = new List<IAccountType>(
        [
            new AccountTypeBase { Value = "Depository", Parent = "" },
            new AccountTypeBase { Value = "Checking", Parent = "Depository" },
            new AccountTypeBase { Value = "Savings", Parent = "Depository" },
            new AccountTypeBase { Value = "Money Market", Parent = "Depository" },
            new AccountTypeBase { Value = "Certificate of Deposit", Parent = "Depository" },
            new AccountTypeBase { Value = "Credit", Parent = "" },
            new AccountTypeBase { Value = "Credit Card", Parent = "Credit" },
            new AccountTypeBase { Value = "Investment", Parent = "" },
            new AccountTypeBase { Value = "Brokerage", Parent = "Investment" },
            new AccountTypeBase { Value = "Retirement", Parent = "Investment" },
            new AccountTypeBase { Value = "529", Parent = "Investment" },
            new AccountTypeBase { Value = "Loan", Parent = "" },
            new AccountTypeBase { Value = "Mortgage", Parent = "Loan" },
            new AccountTypeBase { Value = "Auto Loan", Parent = "Loan" },
            new AccountTypeBase { Value = "Student Loan", Parent = "Loan" },
            new AccountTypeBase { Value = "Personal Loan", Parent = "Loan" },
            new AccountTypeBase { Value = "Other", Parent = "" },
            new AccountTypeBase { Value = "Cash", Parent = "Other" },
        ]
    );
}
