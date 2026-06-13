using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
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
public class AccountServiceTests()
{
    private readonly Faker<AccountUpdateRequest> _accountUpdateRequestFaker =
        new Faker<AccountUpdateRequest>()
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.Type, f => f.Finance.TransactionType())
            .RuleFor(a => a.HideTransactions, f => true)
            .RuleFor(a => a.HideAccount, f => true)
            .RuleFor(a => a.InterestRate, f => f.Finance.Amount(0, 0.25M))
            .RuleFor(a => a.Source, f => f.PickRandom<string>(AccountSource.AllowedValues));

    #region CreateAccountAsync
    [Fact]
    public async Task CreateAccountAsync_WhenRequestIsValid_ShouldCreateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var account = new AccountCreateRequest
        {
            Name = "My Account",
            InstitutionID = institution.ID,
        };

        // Act
        await accountService.CreateAccountAsync(helper.demoUser.Id, account);

        // Assert
        helper.demoUser.Accounts.Should().HaveCount(1);
        helper.demoUser.Accounts.Single().Name.Should().Be(account.Name);
        helper.demoUser.Accounts.Single().InstitutionID.Should().Be(account.InstitutionID);
        helper.demoUser.Accounts.Single().Source.Should().Be(AccountSource.Manual);
        helper.demoUser.Accounts.Single().UserID.Should().Be(helper.demoUser.Id);
    }

    [Fact]
    public async Task CreateAccountAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var account = new AccountCreateRequest
        {
            Name = "My Account",
            InstitutionID = institution.ID,
        };

        // Act
        var createAccountAct = () => accountService.CreateAccountAsync(Guid.NewGuid(), account);

        // Assert
        await createAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateAccountAsync_InvalidInstitutionId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var account = new AccountCreateRequest
        {
            Name = "My Account",
            InstitutionID = Guid.NewGuid(),
        };

        // Act
        var createAccountAct = () => accountService.CreateAccountAsync(helper.demoUser.Id, account);

        // Assert
        await createAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidInstitutionIDError");
    }

    [Fact]
    public async Task CreateAccountAsync_WhenInstitutionDeleted_ShouldRestoreInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();
        institution.Deleted = new Faker().Date.Past().ToUniversalTime();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var account = new AccountCreateRequest
        {
            Name = "My Account",
            InstitutionID = institution.ID,
        };

        // Act
        await accountService.CreateAccountAsync(helper.demoUser.Id, account);

        // Assert
        helper.demoUser.Accounts.Should().HaveCount(1);
        helper.demoUser.Institutions.Single(i => i.ID == institution.ID).Deleted.Should().BeNull();
    }
    #endregion

    #region ReadAccountsAsync
    [Fact]
    public async Task ReadAccountsAsync_WhenAccounts_ShouldReturnOrderedAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var firstAccount = accountFaker.Generate();
        firstAccount.Index = 0;

        var secondAccount = accountFaker.Generate();
        secondAccount.Index = 1;

        helper.UserDataContext.Accounts.AddRange(secondAccount, firstAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await accountService.ReadAccountsAsync(helper.demoUser.Id);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(new AccountResponse(firstAccount));
        result[1].Should().BeEquivalentTo(new AccountResponse(secondAccount));
    }
    #endregion

    #region UpdateAccountAsync
    [Fact]
    public async Task UpdateAccountAsync_ExistingAccount_ShouldUpdateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedAccount = _accountUpdateRequestFaker.Generate();
        editedAccount.ID = account.ID;

        // Act
        await accountService.UpdateAccountAsync(helper.demoUser.Id, editedAccount);

        // Assert
        account.Should().BeEquivalentTo(editedAccount);
    }

    [Fact]
    public async Task UpdateAccountAsync_InvalidAccount_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedAccount = _accountUpdateRequestFaker.Generate();
        editedAccount.ID = Guid.NewGuid();

        // Act
        var updateAccountAct = () =>
            accountService.UpdateAccountAsync(helper.demoUser.Id, editedAccount);

        // Assert
        await updateAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountEditNotFoundError");
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenValueIsNull_DoesNotUpdateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var originalAccount = new Account
        {
            ID = account.ID,
            Name = account.Name,
            Type = account.Type,
            HideTransactions = account.HideTransactions,
            HideAccount = account.HideAccount,
            InterestRate = account.InterestRate,
            Source = account.Source,
            Index = account.Index,
            Deleted = account.Deleted,
            InstitutionID = account.InstitutionID,
            UserID = account.UserID,
        };

        var editedAccount = new AccountUpdateRequest
        {
            ID = account.ID,
            Name = null,
            Type = null,
            HideTransactions = null,
            HideAccount = null,
            InterestRate = null,
            Source = null,
        };

        // Act
        await accountService.UpdateAccountAsync(helper.demoUser.Id, editedAccount);

        // Assert
        helper
            .UserDataContext.Accounts.Single(a => a.ID == account.ID)
            .Should()
            .BeEquivalentTo(
                originalAccount,
                options =>
                    options
                        .Excluding(a => a.User)
                        .Excluding(a => a.Institution)
                        .Excluding(a => a.Transactions)
                        .Excluding(a => a.Goals)
                        .Excluding(a => a.Balances)
                        .Excluding(a => a.SimpleFinAccount)
                        .Excluding(a => a.LunchFlowAccount)
            );
    }

    [Fact]
    public async Task UpdateAccountAsync_WhenInvalidSource_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedAccount = new AccountUpdateRequest { ID = account.ID, Source = "InvalidSource" };

        // Act
        var updateAccountAct = () =>
            accountService.UpdateAccountAsync(helper.demoUser.Id, editedAccount);

        // Assert
        await updateAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidAccountSourceError");
    }

    #endregion

    #region DeleteAccountAsync
    [Fact]
    public async Task DeleteAccountAsync_ExistingAccount_ShouldDeleteAccount()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.Now).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.DeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        var deletedAccount = helper.demoUser.Accounts.Single(a => a.ID == account.ID);
        deletedAccount.Deleted.Should().Be(fakeDate);
        deletedAccount.Source.Should().Be(AccountSource.Manual);
        deletedAccount.Type.Should().Be(string.Empty);
    }

    [Fact]
    public async Task DeleteAccountAsync_InvalidAccount_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var deleteAccountAct = () =>
            accountService.DeleteAccountAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await deleteAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountDeleteNotFoundError");
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenDeleteTransactionsIsTrue_ShouldDeleteTransactions()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.Now).Returns(fakeDate);

        var transactionServiceMock = new Mock<ITransactionService>();

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();
        transaction.AccountID = account.ID;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.DeleteAccountAsync(helper.demoUser.Id, account.ID, true);

        // Assert
        helper.demoUser.Accounts.Single(a => a.ID == account.ID).Deleted.Should().Be(fakeDate);
        var deletedTransaction = helper
            .demoUser.Accounts.Single(a => a.ID == account.ID)
            .Transactions.Single(t => t.ID == transaction.ID);
        transactionServiceMock.Verify(
            ts =>
                ts.DeleteTransactionBatchAsync(helper.demoUser.Id, new[] { transaction.ID }, true),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteAccountAsync_DeleteLastAccount_ShouldDeleteInstitution()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.Now).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.InstitutionID = institution.ID;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.DeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper
            .demoUser.Institutions.Single(i => i.ID == institution.ID)
            .Deleted.Should()
            .Be(fakeDate);
    }

    [Fact]
    public async Task DeleteAccountAsync_DeleteNotLastAccount_ShouldNotDeleteInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.InstitutionID = institution.ID;

        var secondAccount = accountFaker.Generate();
        secondAccount.InstitutionID = institution.ID;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Accounts.Add(secondAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.DeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.demoUser.Institutions.Single(i => i.ID == institution.ID).Deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAccountAsync_WithLinkedLunchFlowAccount_ShouldClearLink()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = account.ID;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.DeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.demoUser.Accounts.Single(a => a.ID == account.ID).Deleted.Should().NotBeNull();
        helper
            .UserDataContext.LunchFlowAccounts.Single(a => a.ID == lunchFlowAccount.ID)
            .LinkedAccountId.Should()
            .BeNull();
        helper
            .UserDataContext.LunchFlowAccounts.Single(a => a.ID == lunchFlowAccount.ID)
            .LastSync.Should()
            .BeNull();
    }

    [Fact]
    public async Task DeleteAccountAsync_WithLinkedSimpleFinAccount_ShouldClearLink()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var orgFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var org = orgFaker.Generate();

        var simpleFinAccountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, org.ID);
        var simpleFinAccount = simpleFinAccountFaker.Generate();
        simpleFinAccount.LinkedAccountId = account.ID;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SimpleFinOrganizations.Add(org);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.DeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.demoUser.Accounts.Single(a => a.ID == account.ID).Deleted.Should().NotBeNull();
        helper
            .UserDataContext.SimpleFinAccounts.Single(a => a.ID == simpleFinAccount.ID)
            .LinkedAccountId.Should()
            .BeNull();
        helper
            .UserDataContext.SimpleFinAccounts.Single(a => a.ID == simpleFinAccount.ID)
            .LastSync.Should()
            .BeNull();
    }
    #endregion

    #region RestoreAccountAsync
    [Fact]
    public async Task RestoreAccountAsync_ExistingAccount_ShouldRestoreAccount()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Deleted = fakeDate;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.RestoreAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.demoUser.Accounts.Single(a => a.ID == account.ID).Deleted.Should().BeNull();
    }

    [Fact]
    public async Task RestoreAccountAsync_InvalidAccount_ThrowsError()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Deleted = fakeDate;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var invalidGuid = Guid.NewGuid();

        // Act
        var restoreAccountAct = () =>
            accountService.RestoreAccountAsync(helper.demoUser.Id, invalidGuid);

        // Assert
        await restoreAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountRestoreNotFoundError");
    }

    [Fact]
    public async Task RestoreAccountAsync_WhenRestoreTransactionsIsTrue_ShouldRestoreTransactions()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var transactionServiceMock = new Mock<ITransactionService>();

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            transactionServiceMock.Object,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Deleted = fakeDate;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transaction = transactionFaker.Generate();

        transaction.Deleted = fakeDate;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.RestoreAccountAsync(helper.demoUser.Id, account.ID, true);

        // Assert
        helper.demoUser.Accounts.Single(a => a.ID == account.ID).Deleted.Should().BeNull();
        transactionServiceMock.Verify(
            ts =>
                ts.RestoreTransactionBatchAsync(helper.demoUser.Id, new[] { transaction.ID }, true),
            Times.Once
        );
    }

    [Fact]
    public async Task RestoreAccountAsync_RestoreAccount_ShouldRestoreInstitution()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();
        institution.Deleted = fakeDate;

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.InstitutionID = institution.ID;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.RestoreAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.demoUser.Institutions.Single(i => i.ID == institution.ID).Deleted.Should().BeNull();
    }
    #endregion

    #region OrderAccountsAsync
    [Fact]
    public async Task OrderAccountsAsync_WhenExistingAccounts_ShouldOrderAccounts()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(10);
        var rnd = new Random();
        accounts = [.. accounts.OrderBy(a => rnd.Next())];
        for (int i = 0; i < accounts.Count; i++)
        {
            accounts[i].Index = i;
        }

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        var orderedAccounts = new List<IAccountIndexRequest>();
        List<Account> shuffledAccounts = [.. accounts.OrderBy(a => Guid.NewGuid())];
        foreach (var account in shuffledAccounts)
        {
            orderedAccounts.Add(
                new AccountIndexRequest { ID = account.ID, Index = accounts.IndexOf(account) }
            );
        }

        // Act
        await accountService.OrderAccountsAsync(helper.demoUser.Id, orderedAccounts);

        // Assert
        helper
            .demoUser.Accounts.OrderBy(a => a.Index)
            .Select(a => a.ID)
            .Should()
            .BeEquivalentTo(orderedAccounts.OrderBy(o => o.Index).Select(o => o.ID));
    }

    [Fact]
    public async Task OrderAccountsAsync_InvalidAccount_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var accounts = accountFaker.Generate(10);

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        List<IAccountIndexRequest> orderedAccounts = [];
        foreach (var account in accounts)
        {
            orderedAccounts.Add(
                new AccountIndexRequest { ID = account.ID, Index = accounts.IndexOf(account) }
            );
        }

        var invalidGuid = Guid.NewGuid();
        orderedAccounts.Add(new AccountIndexRequest { ID = invalidGuid, Index = accounts.Count });

        // Act
        var orderAccountsAct = () =>
            accountService.OrderAccountsAsync(helper.demoUser.Id, orderedAccounts);

        // Assert
        await orderAccountsAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountOrderNotFoundError");
    }
    #endregion

    #region PermanentlyDeleteAccountAsync
    [Fact]
    public async Task PermanentlyDeleteAccountAsync_ExistingAccount_ShouldRemoveAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.PermanentlyDeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.UserDataContext.Accounts.Should().NotContain(a => a.ID == account.ID);
    }

    [Fact]
    public async Task PermanentlyDeleteAccountAsync_InvalidAccount_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var act = () =>
            accountService.PermanentlyDeleteAccountAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountPermanentDeleteNotFoundError");
    }

    [Fact]
    public async Task PermanentlyDeleteAccountAsync_WithTransactionsAndBalances_ShouldRemoveAll()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(3);

        var balanceFaker = new BalanceFaker([account.ID]);
        var balances = balanceFaker.Generate(3);

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Transactions.AddRange(transactions);
        helper.UserDataContext.Balances.AddRange(balances);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.PermanentlyDeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.UserDataContext.Accounts.Should().NotContain(a => a.ID == account.ID);
        helper
            .UserDataContext.Transactions.Should()
            .NotContain(t => transactions.Select(tx => tx.ID).Contains(t.ID));
        helper
            .UserDataContext.Balances.Should()
            .NotContain(b => balances.Select(bl => bl.ID).Contains(b.ID));
    }

    [Fact]
    public async Task PermanentlyDeleteAccountAsync_WithLinkedLunchFlowAccount_ShouldRemoveAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var lunchFlowAccountFaker = new LunchFlowAccountFaker(helper.demoUser.Id);
        var lunchFlowAccount = lunchFlowAccountFaker.Generate();
        lunchFlowAccount.LinkedAccountId = account.ID;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.LunchFlowAccounts.Add(lunchFlowAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.PermanentlyDeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.UserDataContext.Accounts.Should().NotContain(a => a.ID == account.ID);
    }

    [Fact]
    public async Task PermanentlyDeleteAccountAsync_WithLinkedSimpleFinAccount_ShouldRemoveAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<ITransactionService>(),
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();

        var orgFaker = new SimpleFinOrganizationFaker(helper.demoUser.Id);
        var org = orgFaker.Generate();

        var simpleFinAccountFaker = new SimpleFinAccountFaker(helper.demoUser.Id, org.ID);
        var simpleFinAccount = simpleFinAccountFaker.Generate();
        simpleFinAccount.LinkedAccountId = account.ID;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SimpleFinOrganizations.Add(org);
        helper.UserDataContext.SimpleFinAccounts.Add(simpleFinAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.PermanentlyDeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.UserDataContext.Accounts.Should().NotContain(a => a.ID == account.ID);
    }
    #endregion
}
