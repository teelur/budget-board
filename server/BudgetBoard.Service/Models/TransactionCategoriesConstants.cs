namespace BudgetBoard.Service.Models;

public static class TransactionCategoriesConstants
{
    public const string HideFromBudgetsCategory = "Hide from Budgets";

    public static readonly IReadOnlyCollection<ITransactionCategory> SpecialTransactionCategories =
        new List<ITransactionCategory>(
            [
                new TransactionCategoryBase
                {
                    Value = HideFromBudgetsCategory,
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
            ]
        );
    public static readonly IReadOnlyCollection<ITransactionCategory> DefaultTransactionCategories =
        new List<ITransactionCategory>(
            [
                new TransactionCategoryBase
                {
                    Value = "Auto & Transport",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Auto Insurance",
                    Parent = "Auto & Transport",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Auto Payment",
                    Parent = "Auto & Transport",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Gas & Fuel",
                    Parent = "Auto & Transport",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Parking",
                    Parent = "Auto & Transport",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Public Transportation",
                    Parent = "Auto & Transport",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Ride Share",
                    Parent = "Auto & Transport",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Service & Parts",
                    Parent = "Auto & Transport",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Bills & Utilities",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Internet",
                    Parent = "Bills & Utilities",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Mobile Phone",
                    Parent = "Bills & Utilities",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Television",
                    Parent = "Bills & Utilities",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Utilities",
                    Parent = "Bills & Utilities",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Education",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Books & Supplies",
                    Parent = "Education",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Student Loan",
                    Parent = "Education",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Tuition",
                    Parent = "Education",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Entertainment",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Activities",
                    Parent = "Entertainment",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Arts",
                    Parent = "Entertainment",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Movies",
                    Parent = "Entertainment",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Music",
                    Parent = "Entertainment",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Books",
                    Parent = "Entertainment",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Games",
                    Parent = "Entertainment",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Fees & Charges",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "ATM Fee",
                    Parent = "Fees & Charges",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Bank Fee",
                    Parent = "Fees & Charges",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Finance Charge",
                    Parent = "Fees & Charges",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Late Fee",
                    Parent = "Fees & Charges",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Service Fee",
                    Parent = "Fees & Charges",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Trade Commissions",
                    Parent = "Fees & Charges",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Financial",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Roth IRA",
                    Parent = "Financial",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Investment",
                    Parent = "Financial",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Food & Dining",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Alcohol & Bars",
                    Parent = "Food & Dining",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Coffee Shops",
                    Parent = "Food & Dining",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Food Delivery",
                    Parent = "Food & Dining",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Groceries",
                    Parent = "Food & Dining",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Restaurants",
                    Parent = "Food & Dining",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Gifts & Donations",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Charity",
                    Parent = "Gifts & Donations",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Gift",
                    Parent = "Gifts & Donations",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Goals",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Health & Fitness",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Dentist",
                    Parent = "Health & Fitness",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Doctor",
                    Parent = "Health & Fitness",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Eyecare",
                    Parent = "Health & Fitness",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Gym",
                    Parent = "Health & Fitness",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Health Insurance",
                    Parent = "Health & Fitness",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Pharmacy",
                    Parent = "Health & Fitness",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Sports",
                    Parent = "Health & Fitness",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Home",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Furnishings",
                    Parent = "Home",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Home Improvement",
                    Parent = "Home",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Home Insurance",
                    Parent = "Home",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Home Services",
                    Parent = "Home",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Home Supplies",
                    Parent = "Home",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Lawn & Garden",
                    Parent = "Home",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Mortgage & Rent",
                    Parent = "Home",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Income",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Income,
                },
                new TransactionCategoryBase
                {
                    Value = "Bonus",
                    Parent = "Income",
                    CategoryType = TransactionCategoryTypes.Income,
                },
                new TransactionCategoryBase
                {
                    Value = "Interest Income",
                    Parent = "Income",
                    CategoryType = TransactionCategoryTypes.Income,
                },
                new TransactionCategoryBase
                {
                    Value = "Paycheck",
                    Parent = "Income",
                    CategoryType = TransactionCategoryTypes.Income,
                },
                new TransactionCategoryBase
                {
                    Value = "Reimbursements",
                    Parent = "Income",
                    CategoryType = TransactionCategoryTypes.Income,
                },
                new TransactionCategoryBase
                {
                    Value = "Tax Return",
                    Parent = "Income",
                    CategoryType = TransactionCategoryTypes.Income,
                },
                new TransactionCategoryBase
                {
                    Value = "Investments",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Buy",
                    Parent = "Investments",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Deposit",
                    Parent = "Investments",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Dividend & Cap Gains",
                    Parent = "Investments",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Sell",
                    Parent = "Investments",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Withdrawl",
                    Parent = "Investments",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Loans",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Loan Fees & Charges",
                    Parent = "Loans",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Loan Insurance",
                    Parent = "Loans",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Loan Interest",
                    Parent = "Loans",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Loan Payments",
                    Parent = "Loans",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Loan Principal",
                    Parent = "Loans",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Misc",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Personal Care",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Hair",
                    Parent = "Personal Care",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Laundry",
                    Parent = "Personal Care",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Spa & Massage",
                    Parent = "Personal Care",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Pets",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Pet Food & Supplies",
                    Parent = "Pets",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Pet Grooming",
                    Parent = "Pets",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Veterinary",
                    Parent = "Pets",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Shopping",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Clothing",
                    Parent = "Shopping",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Electronics & Software",
                    Parent = "Shopping",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Hobbies",
                    Parent = "Shopping",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Household",
                    Parent = "Shopping",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Taxes",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Federal Tax",
                    Parent = "Taxes",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Local Tax",
                    Parent = "Taxes",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Property Tax",
                    Parent = "Taxes",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Sales Tax",
                    Parent = "Taxes",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "State Tax",
                    Parent = "Taxes",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Transfer",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Credit Card Payment",
                    Parent = "Transfer",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Travel",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Air Travel",
                    Parent = "Travel",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Hotel",
                    Parent = "Travel",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Rental Car & Taxi",
                    Parent = "Travel",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Vacation",
                    Parent = "Travel",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
                new TransactionCategoryBase
                {
                    Value = "Other",
                    Parent = "",
                    CategoryType = TransactionCategoryTypes.Expense,
                },
            ]
        );
}
