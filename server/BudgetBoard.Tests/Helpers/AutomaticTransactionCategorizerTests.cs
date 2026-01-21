using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;
using FluentAssertions;

namespace BudgetBoard.IntegrationTests.Helpers;

[Collection("IntegrationTests")]
public class AutomaticTransactionCategorizerTests
{
    private Account account;
    
    public AutomaticTransactionCategorizerTests()
    {
        account = new Account
        {
            Name = "account name",
            UserID = new Guid("dddddddddddddddddddddddddddddddd"),
            ID = new Guid("dddddddddddddddddddddddddddddddd")
        };

        // Create transactions to be used to train the model.
        account.Transactions.Add(
            new Transaction
            {
                Amount = 1.0M,
                Date = DateTime.Parse("2025-01-01"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "abc def ghi",
                Category = "Category1"
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 37.76M,
                Date = DateTime.Parse("2025-01-02"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "jkl mno pqr",
                Category = "Category2"
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 10000.00M,
                Date = DateTime.Parse("2025-01-03"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "stu wv xyz",
                Category = "Category3"
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 1.05M,
                Date = DateTime.Parse("2025-01-04"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "xyz a bc",
                Category = "Category4"
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 10.0M,
                Date = DateTime.Parse("2025-01-03"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "abc x a g",
                Category = "Category1"
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 1.0M,
                Date = DateTime.Parse("2025-01-06"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "bg xyz a bc",
                Category = "Category4"
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 124.86M,
                Date = DateTime.Parse("2025-01-02"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "jkl mno pqr",
                Category = "Category5"
            }
        );
    }

    [Fact]
    public void AutomaticTransactionCategorizer_WhenTwoMatches_ShouldReturnClosestAmount()
    {
        // Arrange
        var mlModel = AutomaticTransactionCategorizer.Train(account.Transactions);
        AutomaticTransactionCategorizer autoCategorizer = new AutomaticTransactionCategorizer(mlModel);

        var newTransaction1 = new Transaction
        {
            Amount = 21.49M,
            Date = DateTime.Parse("2025-02-01"),
            Account = account,
            AccountID = account.ID,
            MerchantName = "jkl mno pqr",
            Source = "foo",
            Category = ""
        };

        var newTransaction2 = new Transaction
        {
            Amount = 129.23M,
            Date = DateTime.Parse("2025-02-01"),
            Account = account,
            AccountID = account.ID,
            MerchantName = "jkl mno pqr",
            Source = "foo",
            Category = ""
        };

        // Act
        var newCategory1 = autoCategorizer.Predict(newTransaction1);
        var newCategory2 = autoCategorizer.Predict(newTransaction2);

        // Assert
        newCategory1.Should().Be("Category2");
        newCategory2.Should().Be("Category5");
    }
}