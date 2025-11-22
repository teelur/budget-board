using Bogus;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AccountServiceTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    private readonly Faker<AccountCreateRequest> _accountCreateRequestFaker =
        new Faker<AccountCreateRequest>()
            .RuleFor(a => a.SyncID, f => f.Random.String(20))
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.InstitutionID, f => Guid.NewGuid())
            .RuleFor(a => a.Type, f => f.Finance.TransactionType())
            .RuleFor(a => a.Subtype, f => f.Finance.TransactionType())
            .RuleFor(a => a.HideTransactions, f => false)
            .RuleFor(a => a.HideAccount, f => false)
            .RuleFor(a => a.Source, f => AccountSource.Manual);

    private readonly Faker<AccountUpdateRequest> _accountUpdateRequestFaker =
        new Faker<AccountUpdateRequest>()
            .RuleFor(a => a.Name, f => f.Finance.AccountName())
            .RuleFor(a => a.Type, f => f.Finance.TransactionType())
            .RuleFor(a => a.Subtype, f => f.Finance.TransactionType())
            .RuleFor(a => a.HideTransactions, f => false)
            .RuleFor(a => a.HideAccount, f => false);

    [Fact]
    public async Task CreateAccountAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();

        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var account = _accountCreateRequestFaker.Generate();
        account.InstitutionID = institution.ID;

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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var account = _accountCreateRequestFaker.Generate();
        account.InstitutionID = Guid.NewGuid();

        // Act
        var createAccountAct = () => accountService.CreateAccountAsync(helper.demoUser.Id, account);

        // Assert
        await createAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidInstitutionIDError");
    }

    [Fact]
    public async Task CreateAccountAsync_DuplicateName_ShouldStillCreateAccount()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.InstitutionID = institution.ID;
        account.Name = "Test Account";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var duplicateAccount = _accountCreateRequestFaker.Generate();
        duplicateAccount.Name = account.Name;
        duplicateAccount.InstitutionID = institution.ID;

        // Act
        await accountService.CreateAccountAsync(helper.demoUser.Id, duplicateAccount);

        // Assert
        helper.demoUser.Accounts.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAccountAsync_WhenDuplicateSyncID_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.InstitutionID = institution.ID;
        account.SyncID = "TestSyncID";

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var duplicateAccount = _accountCreateRequestFaker.Generate();
        duplicateAccount.SyncID = account.SyncID;
        duplicateAccount.InstitutionID = institution.ID;

        // Act
        var createAccountAct = () =>
            accountService.CreateAccountAsync(helper.demoUser.Id, duplicateAccount);

        // Assert
        await createAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("DuplicateSyncIDError");
    }

    [Fact]
    public async Task CreateAccountAsync_WhenSyncIDNull_ShouldNotFlagAsDuplicate()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.InstitutionID = institution.ID;
        account.SyncID = null;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var duplicateAccount = _accountCreateRequestFaker.Generate();
        duplicateAccount.SyncID = null;
        duplicateAccount.InstitutionID = institution.ID;

        // Act
        await accountService.CreateAccountAsync(helper.demoUser.Id, duplicateAccount);

        // Assert
        helper.demoUser.Accounts.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAccountAsync_NewAccount_HappyPath()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();

        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var account = _accountCreateRequestFaker.Generate();
        account.InstitutionID = institution.ID;

        // Act
        await accountService.CreateAccountAsync(helper.demoUser.Id, account);

        // Assert
        helper.demoUser.Accounts.Should().HaveCount(1);

        helper.demoUser.Accounts.Single().Should().BeEquivalentTo(account);
    }

    [Fact]
    public async Task ReadAccountsAsync_ReadAll_HappyPath()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await accountService.ReadAccountsAsync(helper.demoUser.Id);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Should().BeEquivalentTo(new AccountResponse(account));
    }

    [Fact]
    public async Task ReadAccountsAsync_ReadSingle_HappyPath()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        var secondAccount = accountFaker.Generate();
        secondAccount.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.Accounts.Add(secondAccount);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await accountService.ReadAccountsAsync(helper.demoUser.Id, account.ID);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Should().BeEquivalentTo(new AccountResponse(account));
    }

    [Fact]
    public async Task ReadAccountsAsync_ReadInvalid_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var invalidGuid = Guid.NewGuid();

        // Act
        var readAccountAct = () =>
            accountService.ReadAccountsAsync(helper.demoUser.Id, invalidGuid);

        // Assert
        await readAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountNotFoundError");
    }

    [Fact]
    public async Task UpdateAccountAsync_ExistingAccount_HappyPath()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var editedAccount = _accountUpdateRequestFaker.Generate();

        var invalidGuid = Guid.NewGuid();

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
    public async Task DeleteAccountAsync_ExistingAccount_HappyPath()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.DeleteAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.demoUser.Accounts.Single(a => a.ID == account.ID).Deleted.Should().Be(fakeDate);
    }

    [Fact]
    public async Task DeleteAccountAsync_InvalidAccount_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );
        var invalidGuid = Guid.NewGuid();

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        var deleteAccountAct = () =>
            accountService.DeleteAccountAsync(helper.demoUser.Id, invalidGuid);

        // Assert
        await deleteAccountAct
            .Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountDeleteNotFoundError");
    }

    [Fact]
    public async Task DeleteAccountAsync_DeleteTransactions_HappyPath()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;

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
        helper
            .demoUser.Accounts.Single(a => a.ID == account.ID)
            .Transactions.Single(t => t.ID == transaction.ID)
            .Deleted.Should()
            .Be(fakeDate);
    }

    [Fact]
    public async Task DeleteAccountAsync_DeleteLastAccount_ShouldDeleteInstitution()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.InstitutionID = institution.ID;

        var secondAccount = accountFaker.Generate();
        secondAccount.UserID = helper.demoUser.Id;
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
    public async Task RestoreAccountAsync_ExistingAccount_HappyPath()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
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
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
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
    public async Task RestoreAccountAsync_RestoreTransactions_HappyPath()
    {
        // Arrange
        var fakeDate = new Faker().Date.Past().ToUniversalTime();

        var nowProviderMock = new Mock<INowProvider>();
        nowProviderMock.Setup(np => np.UtcNow).Returns(fakeDate);

        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
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
        helper
            .demoUser.Accounts.Single(a => a.ID == account.ID)
            .Transactions.Single(t => t.ID == transaction.ID)
            .Deleted.Should()
            .BeNull();
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
            nowProviderMock.Object,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;
        institution.Deleted = fakeDate;

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.InstitutionID = institution.ID;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountService.RestoreAccountAsync(helper.demoUser.Id, account.ID);

        // Assert
        helper.demoUser.Institutions.Single(i => i.ID == institution.ID).Deleted.Should().BeNull();
    }

    [Fact]
    public async Task OrderAccountsAsync_ExistingAccounts_HappyPath()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var randomNumberBetween1And10 = new Random().Next(1, 10);
        _testOutputHelper.WriteLine($"Number of accounts: {randomNumberBetween1And10}");

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(randomNumberBetween1And10);
        foreach (var account in accounts)
        {
            account.UserID = helper.demoUser.Id;
        }

        helper.UserDataContext.Accounts.AddRange(accounts);
        helper.UserDataContext.SaveChanges();

        List<IAccountIndexRequest> orderedAccounts = [];
        foreach (var account in accounts)
        {
            orderedAccounts.Add(
                new AccountIndexRequest { ID = account.ID, Index = accounts.IndexOf(account) }
            );
        }

        // Act
        await accountService.OrderAccountsAsync(helper.demoUser.Id, orderedAccounts);

        // Assert
        foreach (var account in accounts)
        {
            helper
                .demoUser.Accounts.Single(a => a.ID == account.ID)
                .Should()
                .BeEquivalentTo(account);
        }
    }

    [Fact]
    public async Task OrderAccountsAsync_InvalidAccount_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var accountService = new AccountService(
            Mock.Of<ILogger<IAccountService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var randomNumberBetween1And10 = new Random().Next(1, 10);
        _testOutputHelper.WriteLine($"Number of accounts: {randomNumberBetween1And10}");

        var accountFaker = new AccountFaker();
        var accounts = accountFaker.Generate(randomNumberBetween1And10);
        foreach (var account in accounts)
        {
            account.UserID = helper.demoUser.Id;
        }

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
}
