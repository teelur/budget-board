using BudgetBoard.Database.Interfaces;
using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Moq;

namespace BudgetBoard.IntegrationTests.Helpers;

[Collection("IntegrationTests")]
public class AutomaticTransactionCategorizerTests
{
    private readonly Account account;

    public AutomaticTransactionCategorizerTests()
    {
        account = new Account
        {
            Name = "account name",
            UserID = new Guid("dddddddddddddddddddddddddddddddd"),
            ID = new Guid("dddddddddddddddddddddddddddddddd"),
            InstitutionID = Guid.NewGuid(),
        };

        // Create transactions to be used to train the model.
        account.Transactions.Add(
            new Transaction
            {
                Amount = 1.0M,
                Date = DateOnly.Parse("2025-01-01"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "abc def ghi",
                Category = "Category1",
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 37.76M,
                Date = DateOnly.Parse("2025-01-02"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "jkl mno pqr",
                Category = "Category2",
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 10000.00M,
                Date = DateOnly.Parse("2025-01-03"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "stu wv xyz",
                Category = "Category3",
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 1.05M,
                Date = DateOnly.Parse("2025-01-04"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "xyz a bc",
                Category = "Category4",
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 10.0M,
                Date = DateOnly.Parse("2025-01-03"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "abc x a g",
                Category = "Category1",
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 1.0M,
                Date = DateOnly.Parse("2025-01-06"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "bg xyz a bc",
                Category = "Category4",
            }
        );
        account.Transactions.Add(
            new Transaction
            {
                Amount = 124.86M,
                Date = DateOnly.Parse("2025-01-02"),
                Source = "test",
                Account = account,
                AccountID = account.ID,
                MerchantName = "jkl mno pqr",
                Category = "Category5",
            }
        );
    }

    [Fact]
    public void AutomaticTransactionCategorizer_WhenTwoMatches_ShouldReturnClosestAmount()
    {
        // Arrange
        var mlModel = AutomaticTransactionCategorizerHelper.Train(account.Transactions);
        AutomaticTransactionCategorizerHelper autoCategorizer = new(mlModel);

        var newTransaction1 = new Transaction
        {
            Amount = 21.49M,
            Date = DateOnly.Parse("2025-02-01"),
            Account = account,
            AccountID = account.ID,
            MerchantName = "jkl mno pqr",
            Source = "foo",
            Category = "",
        };

        var newTransaction2 = new Transaction
        {
            Amount = 129.23M,
            Date = DateOnly.Parse("2025-02-01"),
            Account = account,
            AccountID = account.ID,
            MerchantName = "jkl mno pqr",
            Source = "foo",
            Category = "",
        };

        // Act
        var (category1, _) = autoCategorizer.PredictCategory(newTransaction1);
        var (category2, _) = autoCategorizer.PredictCategory(newTransaction2);

        // Assert
        category1.Should().Be("Category2");
        category2.Should().Be("Category5");
    }

    [Fact]
    public void AutomaticTransactionCategorizer_WhenSubcategoryIsPresent_UsesSubcategoryAsTrainingLabel()
    {
        account.Transactions.First().Subcategory = "Subcategory1";
        account.Transactions.Skip(1).First().Subcategory = string.Empty;
        account.Transactions.Skip(2).First().MerchantName = null;
        account.Transactions.Skip(3).First().Category = null;

        var mlModel = AutomaticTransactionCategorizerHelper.Train(account.Transactions);

        mlModel.Should().NotBeEmpty();
    }

    [Fact]
    public void CalculatePredictionProbability_WhenScoresAreNull_ReturnsZero()
    {
        var result = AutomaticTransactionCategorizerHelper.CalculatePredictionProbability(null);

        result.Should().Be(0f);
    }

    [Fact]
    public void CalculatePredictionProbability_WhenScoresAreEmpty_ReturnsZero()
    {
        var result = AutomaticTransactionCategorizerHelper.CalculatePredictionProbability([]);

        result.Should().Be(0f);
    }

    [Fact]
    public void CalculatePredictionProbability_WhenScoresArePresent_ReturnsHighestProbability()
    {
        var result = AutomaticTransactionCategorizerHelper.CalculatePredictionProbability(
            [1f, 2f, 3f]
        );

        result.Should().BeApproximately(0.6652f, 0.0001f);
    }

    [Fact]
    public void GetPredictionCategory_WhenLabelIsNull_ReturnsEmptyString()
    {
        var result = AutomaticTransactionCategorizerHelper.GetPredictionCategory(null);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPredictionCategory_WhenLabelIsPresent_ReturnsLabel()
    {
        var result = AutomaticTransactionCategorizerHelper.GetPredictionCategory("Category");

        result.Should().Be("Category");
    }

    [Fact]
    public async Task CreateAutoCategorizerAsync_WhenUserSettingsAreMissing_ReturnsNull()
    {
        var user = new ApplicationUser { Id = account.UserID };
        var store = new Mock<ILargeObjectStore>();

        var result = await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
            store.Object,
            user
        );

        result.Should().BeNull();
        store.Verify(
            largeObjectStore => largeObjectStore.ReadLargeObjectAsync(It.IsAny<long>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAutoCategorizerAsync_WhenAutoCategorizerIsDisabled_ReturnsNull()
    {
        var user = CreateUser(enabled: false);
        var store = new Mock<ILargeObjectStore>();

        var result = await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
            store.Object,
            user
        );

        result.Should().BeNull();
        store.Verify(
            largeObjectStore => largeObjectStore.ReadLargeObjectAsync(It.IsAny<long>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAutoCategorizerAsync_WhenModelOidIsMissing_ReturnsNull()
    {
        var user = CreateUser(modelObjectId: null);
        var store = new Mock<ILargeObjectStore>();

        var result = await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
            store.Object,
            user
        );

        result.Should().BeNull();
        store.Verify(
            largeObjectStore => largeObjectStore.ReadLargeObjectAsync(It.IsAny<long>()),
            Times.Never
        );
    }

    [Fact]
    public async Task CreateAutoCategorizerAsync_WhenModelBytesAreNull_ReturnsNull()
    {
        var user = CreateUser();
        var store = new Mock<ILargeObjectStore>();
        store
            .Setup(largeObjectStore => largeObjectStore.ReadLargeObjectAsync(123))
            .ReturnsAsync((byte[]?)null);

        var result = await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
            store.Object,
            user
        );

        result.Should().BeNull();
        store.Verify(largeObjectStore => largeObjectStore.ReadLargeObjectAsync(123), Times.Once);
    }

    [Fact]
    public async Task CreateAutoCategorizerAsync_WhenModelBytesAreEmpty_ReturnsNull()
    {
        var user = CreateUser();
        var store = new Mock<ILargeObjectStore>();
        store
            .Setup(largeObjectStore => largeObjectStore.ReadLargeObjectAsync(123))
            .ReturnsAsync([]);

        var result = await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
            store.Object,
            user
        );

        result.Should().BeNull();
        store.Verify(largeObjectStore => largeObjectStore.ReadLargeObjectAsync(123), Times.Once);
    }

    [Fact]
    public async Task CreateAutoCategorizerAsync_WhenModelBytesAreValid_ReturnsCategorizer()
    {
        var user = CreateUser();
        var store = new Mock<ILargeObjectStore>();
        var mlModel = AutomaticTransactionCategorizerHelper.Train(account.Transactions);
        store
            .Setup(largeObjectStore => largeObjectStore.ReadLargeObjectAsync(123))
            .ReturnsAsync(mlModel);

        var result = await AutomaticTransactionCategorizerHelper.CreateAutoCategorizerAsync(
            store.Object,
            user
        );

        result.Should().NotBeNull();
        store.Verify(largeObjectStore => largeObjectStore.ReadLargeObjectAsync(123), Times.Once);
    }

    private ApplicationUser CreateUser(bool enabled = true, long? modelObjectId = 123)
    {
        var user = new ApplicationUser { Id = account.UserID };
        user.UserSettings = new UserSettings
        {
            UserID = user.Id,
            EnableAutoCategorizer = enabled,
            AutoCategorizerModelOID = modelObjectId,
        };
        return user;
    }
}
