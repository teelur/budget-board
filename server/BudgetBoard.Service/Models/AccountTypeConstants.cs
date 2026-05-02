namespace BudgetBoard.Service.Models;

public static class AccountTypeConstants
{
    public static readonly IEnumerable<IAccountType> DefaultAccountTypes = new List<IAccountType>(
        [
            new AccountTypeBase
            {
                Value = "Depository",
                Parent = "",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Checking",
                Parent = "Depository",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Savings",
                Parent = "Depository",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Money Market",
                Parent = "Depository",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Certificate of Deposit",
                Parent = "Depository",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Credit",
                Parent = "",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Credit Card",
                Parent = "Credit",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Line of Credit",
                Parent = "Credit",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Loan",
                Parent = "",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Auto",
                Parent = "Loan",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Student",
                Parent = "Loan",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Personal",
                Parent = "Loan",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Home Equity",
                Parent = "Loan",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Mortgage",
                Parent = "Loan",
                Classification = AccountClassifications.Liability,
            },
            new AccountTypeBase
            {
                Value = "Investment",
                Parent = "",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "401k",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Traditional IRA",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Roth IRA",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Rollover IRA",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "529",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "HSA",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "ESPP",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Trust",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Taxable",
                Parent = "Investment",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Cash",
                Parent = "",
                Classification = AccountClassifications.Asset,
            },
            new AccountTypeBase
            {
                Value = "Cryptocurrency",
                Parent = "",
                Classification = AccountClassifications.Asset,
            },
        ]
    );
}
