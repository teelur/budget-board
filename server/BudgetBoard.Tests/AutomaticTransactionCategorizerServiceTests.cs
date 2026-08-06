using BudgetBoard.Database.Interfaces;
using BudgetBoard.Database.Models;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AutomaticTransactionCategorizerServiceTests
{
    private static readonly DateTime FixedNow = new(2026, 8, 5, 12, 34, 56, DateTimeKind.Utc);
    private static readonly byte[] ModelBytes = CreateModel();

    #region TrainCategorizerAsync
    [Fact]
    public async Task TrainCategorizerAsync_WhenUserDoesNotExist_ThrowsInvalidUserError()
    {
        var helper = new TestHelper();
        var service = CreateService(helper, new FakeLargeObjectStore());

        Func<Task> act = async () =>
            await service.TrainCategorizerAsync(Guid.NewGuid(), new TrainAutoCategorizerRequest());

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task TrainCategorizerAsync_WhenUserSettingsDoNotExist_ThrowsUserSettingsNotFoundError()
    {
        var helper = new TestHelper();
        var service = CreateService(helper, new FakeLargeObjectStore());

        Func<Task> act = async () =>
            await service.TrainCategorizerAsync(
                helper.demoUser.Id,
                new TrainAutoCategorizerRequest()
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("UserSettingsNotFoundError");
    }

    [Fact]
    public async Task TrainCategorizerAsync_WhenNoTransactionsAreEligible_ThrowsNoTransactionsError()
    {
        var helper = new TestHelper();
        AddUserSettings(helper);
        var deletedAccount = AddAccount(helper, deleted: true);
        var activeAccount = AddAccount(helper);

        AddTransactions(
            helper,
            deletedAccount,
            [CreateTransaction(deletedAccount, "Valid Merchant", "Valid Category")]
        );
        AddTransactions(
            helper,
            activeAccount,
            [
                CreateTransaction(
                    activeAccount,
                    "Deleted Transaction",
                    "Valid Category",
                    deleted: FixedNow
                ),
                CreateTransaction(activeAccount, "Null Category", null),
                CreateTransaction(activeAccount, "Empty Category", string.Empty),
                CreateTransaction(activeAccount, null, "Valid Category"),
                CreateTransaction(activeAccount, string.Empty, "Valid Category"),
            ]
        );

        var store = new FakeLargeObjectStore();
        var service = CreateService(helper, store);

        Func<Task> act = async () =>
            await service.TrainCategorizerAsync(
                helper.demoUser.Id,
                new TrainAutoCategorizerRequest()
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutoCategorizerTrainingNoTransactions");
        store.WriteCallCount.Should().Be(0);
    }

    [Fact]
    public async Task TrainCategorizerAsync_WhenStartDateExcludesTransactions_ThrowsNoTransactionsError()
    {
        var helper = new TestHelper();
        AddUserSettings(helper);
        var account = AddAccount(helper);
        AddTransactions(
            helper,
            account,
            [CreateTransaction(account, "Merchant", "Category", date: new DateOnly(2025, 1, 1))]
        );
        var service = CreateService(helper, new FakeLargeObjectStore());

        Func<Task> act = async () =>
            await service.TrainCategorizerAsync(
                helper.demoUser.Id,
                new TrainAutoCategorizerRequest { StartDate = new DateOnly(2025, 1, 2) }
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutoCategorizerTrainingNoTransactions");
    }

    [Fact]
    public async Task TrainCategorizerAsync_WhenEndDateExcludesTransactions_ThrowsNoTransactionsError()
    {
        var helper = new TestHelper();
        AddUserSettings(helper);
        var account = AddAccount(helper);
        AddTransactions(
            helper,
            account,
            [CreateTransaction(account, "Merchant", "Category", date: new DateOnly(2025, 1, 2))]
        );
        var service = CreateService(helper, new FakeLargeObjectStore());

        Func<Task> act = async () =>
            await service.TrainCategorizerAsync(
                helper.demoUser.Id,
                new TrainAutoCategorizerRequest { EndDate = new DateOnly(2025, 1, 1) }
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AutoCategorizerTrainingNoTransactions");
    }

    [Fact]
    public async Task TrainCategorizerAsync_WhenDateRangeIsValid_StoresModelAndTrainingMetadata()
    {
        var helper = new TestHelper();
        var settings = AddUserSettings(helper);
        var account = AddAccount(helper);
        AddTransactions(helper, account, CreateTrainingTransactions(account));
        var store = new FakeLargeObjectStore { WriteResult = 901 };
        var service = CreateService(helper, store);

        await service.TrainCategorizerAsync(
            helper.demoUser.Id,
            new TrainAutoCategorizerRequest
            {
                StartDate = new DateOnly(2025, 1, 2),
                EndDate = new DateOnly(2025, 1, 6),
            }
        );

        store.LastWriteObjectId.Should().Be(0);
        store.LastWrittenModel.Should().NotBeNull();
        store.LastWrittenModel.Should().NotBeEmpty();
        settings.AutoCategorizerModelOID.Should().Be(901);
        settings.AutoCategorizerLastTrained.Should().Be(new DateOnly(2026, 8, 5));
        settings.AutoCategorizerModelStartDate.Should().Be(new DateOnly(2025, 1, 2));
        settings.AutoCategorizerModelEndDate.Should().Be(new DateOnly(2025, 1, 6));
    }

    [Fact]
    public async Task TrainCategorizerAsync_WhenModelAlreadyExists_ReplacesExistingModel()
    {
        var helper = new TestHelper();
        AddUserSettings(helper, modelObjectId: 42);
        var account = AddAccount(helper);
        AddTransactions(helper, account, CreateTrainingTransactions(account));
        var store = new FakeLargeObjectStore { WriteResult = 43 };
        var service = CreateService(helper, store);

        await service.TrainCategorizerAsync(helper.demoUser.Id, new TrainAutoCategorizerRequest());

        store.LastWriteObjectId.Should().Be(42);
        store.LastWrittenModel.Should().NotBeNull();
        helper.demoUser.UserSettings!.AutoCategorizerModelOID.Should().Be(43);
    }
    #endregion

    #region AutoCategorizeTransactionAsync
    [Fact]
    public async Task AutoCategorizeTransactionAsync_WhenUserDoesNotExist_ThrowsInvalidUserError()
    {
        var helper = new TestHelper();
        var service = CreateService(helper, new FakeLargeObjectStore());

        Func<Task> act = async () =>
            await service.AutoCategorizeTransactionAsync(
                Guid.NewGuid(),
                new Transaction
                {
                    Amount = 1,
                    Date = new DateOnly(2025, 1, 1),
                    Source = "test",
                    AccountID = Guid.NewGuid(),
                    MerchantName = "Merchant",
                }
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task AutoCategorizeTransactionAsync_WhenSettingsAreMissing_LeavesTransactionUnchanged()
    {
        var helper = new TestHelper();
        var store = new FakeLargeObjectStore { ModelToRead = ModelBytes };
        var service = CreateService(helper, store);
        var transaction = CreateTransaction(
            CreateAccountForPrediction(),
            "Merchant",
            "Original",
            subcategory: "Existing"
        );

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Original");
        transaction.Subcategory.Should().Be("Existing");
        store.ReadCallCount.Should().Be(0);
    }

    [Fact]
    public async Task AutoCategorizeTransactionAsync_WhenAutoCategorizerIsDisabled_LeavesTransactionUnchanged()
    {
        var helper = new TestHelper();
        AddUserSettings(helper, modelObjectId: 12);
        var store = new FakeLargeObjectStore { ModelToRead = ModelBytes };
        var service = CreateService(helper, store);
        var transaction = CreateTransaction(
            CreateAccountForPrediction(),
            "Merchant",
            "Original",
            subcategory: "Existing"
        );

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Original");
        transaction.Subcategory.Should().Be("Existing");
        store.ReadCallCount.Should().Be(0);
    }

    [Fact]
    public async Task AutoCategorizeTransactionAsync_WhenModelOidIsMissing_LeavesTransactionUnchanged()
    {
        var helper = new TestHelper();
        AddUserSettings(helper, enabled: true);
        var store = new FakeLargeObjectStore { ModelToRead = ModelBytes };
        var service = CreateService(helper, store);
        var transaction = CreateTransaction(
            CreateAccountForPrediction(),
            "Merchant",
            "Original",
            subcategory: "Existing"
        );

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Original");
        transaction.Subcategory.Should().Be("Existing");
        store.ReadCallCount.Should().Be(0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AutoCategorizeTransactionAsync_WhenStoredModelIsUnavailable_LeavesTransactionUnchanged(
        bool returnNull
    )
    {
        var helper = new TestHelper();
        AddUserSettings(helper, enabled: true, modelObjectId: 12);
        var store = new FakeLargeObjectStore { ModelToRead = returnNull ? null : [] };
        var service = CreateService(helper, store);
        var transaction = CreateTransaction(
            CreateAccountForPrediction(),
            "Merchant",
            "Original",
            subcategory: "Existing"
        );

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Original");
        transaction.Subcategory.Should().Be("Existing");
        store.ReadCallCount.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task AutoCategorizeTransactionAsync_WhenMerchantNameIsMissing_LeavesTransactionUnchanged(
        string? merchantName
    )
    {
        var helper = new TestHelper();
        AddUserSettings(helper, enabled: true, modelObjectId: 12, minimumProbability: 0);
        var store = new FakeLargeObjectStore { ModelToRead = ModelBytes };
        var service = CreateService(helper, store);
        var transaction = CreateTransaction(
            CreateAccountForPrediction(),
            merchantName,
            "Original",
            subcategory: "Existing"
        );

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Original");
        transaction.Subcategory.Should().Be("Existing");
        store.ReadCallCount.Should().Be(1);
    }

    [Fact]
    public async Task AutoCategorizeTransactionAsync_WhenPredictionMeetsThreshold_AssignsFullCategory()
    {
        var helper = new TestHelper();
        AddUserSettings(helper, enabled: true, modelObjectId: 12, minimumProbability: 0);
        var store = new FakeLargeObjectStore { ModelToRead = ModelBytes };
        var service = CreateService(helper, store);
        var transaction = CreateTransaction(
            CreateAccountForPrediction(),
            "abc def ghi",
            "Original",
            amount: 1.0M,
            subcategory: "Existing"
        );

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Auto & Transport");
        transaction.Subcategory.Should().Be("Auto Insurance");
        store.ReadCallCount.Should().Be(1);
    }

    [Fact]
    public async Task AutoCategorizeTransactionAsync_WhenPredictionIsBelowThreshold_PreservesExistingCategory()
    {
        var helper = new TestHelper();
        AddUserSettings(helper, enabled: true, modelObjectId: 12, minimumProbability: 101);
        var store = new FakeLargeObjectStore { ModelToRead = ModelBytes };
        var service = CreateService(helper, store);
        var transaction = CreateTransaction(
            CreateAccountForPrediction(),
            "abc def ghi",
            "Original",
            amount: 1.0M,
            subcategory: "Existing"
        );

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Original");
        transaction.Subcategory.Should().Be("Existing");
        store.ReadCallCount.Should().Be(1);
    }

    [Fact]
    public async Task AutoCategorizeTransactionAsync_WhenAccountNavigationIsMissing_UsesUnknownAccountFallback()
    {
        var helper = new TestHelper();
        AddUserSettings(helper, enabled: true, modelObjectId: 12, minimumProbability: 101);
        var store = new FakeLargeObjectStore { ModelToRead = ModelBytes };
        var service = CreateService(helper, store);
        var transaction = new Transaction
        {
            Amount = 1.0M,
            Date = new DateOnly(2025, 1, 1),
            MerchantName = "abc def ghi",
            Category = "Original",
            Subcategory = "Existing",
            Source = "test",
            AccountID = Guid.NewGuid(),
        };

        await service.AutoCategorizeTransactionAsync(helper.demoUser.Id, transaction);

        transaction.Category.Should().Be("Original");
        transaction.Subcategory.Should().Be("Existing");
        store.ReadCallCount.Should().Be(1);
    }
    #endregion

    private static AutomaticTransactionCategorizerService CreateService(
        TestHelper helper,
        FakeLargeObjectStore largeObjectStore
    )
    {
        var nowProvider = new Mock<INowProvider>();
        nowProvider.SetupGet(provider => provider.Now).Returns(FixedNow);

        return new AutomaticTransactionCategorizerService(
            Mock.Of<ILogger<IAutomaticTransactionCategorizerService>>(),
            helper.UserDataContext,
            largeObjectStore,
            nowProvider.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
    }

    private static UserSettings AddUserSettings(
        TestHelper helper,
        bool enabled = false,
        long? modelObjectId = null,
        int minimumProbability = 70
    )
    {
        var settings = new UserSettings
        {
            UserID = helper.demoUser.Id,
            EnableAutoCategorizer = enabled,
            AutoCategorizerModelOID = modelObjectId,
            AutoCategorizerMinimumProbabilityPercentage = minimumProbability,
        };
        helper.demoUser.UserSettings = settings;
        helper.UserDataContext.UserSettings.Add(settings);
        helper.UserDataContext.SaveChanges();
        return settings;
    }

    private static Account AddAccount(TestHelper helper, bool deleted = false)
    {
        var account = new Account
        {
            Name = "Training Account",
            InstitutionID = Guid.NewGuid(),
            UserID = helper.demoUser.Id,
            Deleted = deleted ? FixedNow : null,
        };
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();
        return account;
    }

    private static void AddTransactions(
        TestHelper helper,
        Account account,
        IEnumerable<Transaction> transactions
    )
    {
        foreach (var transaction in transactions)
        {
            transaction.Account = account;
            transaction.AccountID = account.ID;
        }

        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.SaveChanges();
    }

    private static Transaction CreateTransaction(
        Account account,
        string? merchantName,
        string? category,
        string? subcategory = null,
        decimal amount = 10,
        DateOnly? date = null,
        DateTime? deleted = null
    )
    {
        return new Transaction
        {
            Amount = amount,
            Date = date ?? new DateOnly(2025, 1, 1),
            Category = category,
            Subcategory = subcategory,
            MerchantName = merchantName,
            Deleted = deleted,
            Source = "test",
            AccountID = account.ID,
            Account = account,
        };
    }

    private static List<Transaction> CreateTrainingTransactions(Account account)
    {
        return
        [
            CreateTransaction(
                account,
                "abc def ghi",
                "Auto Insurance",
                amount: 1.0M,
                date: new DateOnly(2025, 1, 1)
            ),
            CreateTransaction(
                account,
                "jkl mno pqr",
                "Gas & Fuel",
                amount: 37.76M,
                date: new DateOnly(2025, 1, 2)
            ),
            CreateTransaction(
                account,
                "stu wv xyz",
                "Education",
                amount: 10000.00M,
                date: new DateOnly(2025, 1, 3)
            ),
            CreateTransaction(
                account,
                "xyz a bc",
                "Entertainment",
                amount: 1.05M,
                date: new DateOnly(2025, 1, 4)
            ),
            CreateTransaction(
                account,
                "abc x a g",
                "Auto Payment",
                amount: 10.0M,
                date: new DateOnly(2025, 1, 5)
            ),
            CreateTransaction(
                account,
                "bg xyz a bc",
                "Books",
                amount: 1.0M,
                date: new DateOnly(2025, 1, 6)
            ),
            CreateTransaction(
                account,
                "jkl mno pqr",
                "Mobile Phone",
                amount: 124.86M,
                date: new DateOnly(2025, 1, 7)
            ),
        ];
    }

    private static Account CreateAccountForPrediction()
    {
        return new Account
        {
            Name = "Categorizer Account",
            InstitutionID = Guid.NewGuid(),
            UserID = Guid.NewGuid(),
        };
    }

    private static byte[] CreateModel()
    {
        return AutomaticTransactionCategorizerHelper.Train(
            CreateTrainingTransactions(CreateAccountForPrediction())
        );
    }

    private sealed class FakeLargeObjectStore : ILargeObjectStore
    {
        public long WriteResult { get; set; } = 100;
        public byte[]? ModelToRead { get; set; }
        public long? LastWriteObjectId { get; private set; }
        public byte[]? LastWrittenModel { get; private set; }
        public int ReadCallCount { get; private set; }
        public int WriteCallCount { get; private set; }

        public Task<long> WriteLargeObjectAsync(long objectId, byte[] data)
        {
            WriteCallCount++;
            LastWriteObjectId = objectId;
            LastWrittenModel = data;
            return Task.FromResult(WriteResult);
        }

        public Task<byte[]?> ReadLargeObjectAsync(long objectId)
        {
            ReadCallCount++;
            return Task.FromResult(ModelToRead);
        }
    }
}
