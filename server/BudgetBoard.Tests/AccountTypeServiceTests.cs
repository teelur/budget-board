using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class AccountTypeServiceTests
{
    private readonly Faker<AccountTypeCreateRequest> _accountTypeCreateRequestFaker =
        new Faker<AccountTypeCreateRequest>()
            .RuleFor(a => a.Value, f => f.Random.String(20))
            .RuleFor(a => a.Parent, f => f.Random.String(20));

    private readonly Faker<AccountTypeUpdateRequest> _accountTypeUpdateRequestFaker =
        new Faker<AccountTypeUpdateRequest>()
            .RuleFor(a => a.Value, f => f.Random.String(20))
            .RuleFor(a => a.Parent, f => f.Random.String(20));

    [Fact]
    public async Task CreateAccountTypeAsync_WhenCalledWithValidData_ShouldCreateAccountType()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var parentAccountType = accountTypeFaker.Generate();

        helper.UserDataContext.AccountTypes.Add(parentAccountType);
        helper.UserDataContext.SaveChanges();

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();
        accountTypeCreateRequest.Parent = parentAccountType.Value;

        // Act
        await accountTypeService.CreateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeCreateRequest
        );

        // Assert
        helper.UserDataContext.AccountTypes.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAccountTypeAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await accountTypeService.CreateAccountTypeAsync(
                Guid.NewGuid(),
                accountTypeCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenCreatingDuplicate_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();

        helper.UserDataContext.AccountTypes.Add(
            new AccountType
            {
                Value = accountTypeCreateRequest.Value,
                Parent = accountTypeCreateRequest.Parent,
                UserID = helper.demoUser.Id,
            }
        );
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await accountTypeService.CreateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeCreateDuplicateNameError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenCreatingEmptyName_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();
        accountTypeCreateRequest.Value = string.Empty;

        // Act
        Func<Task> act = async () =>
            await accountTypeService.CreateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeCreateEmptyNameError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenParentSameAsValue_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();
        accountTypeCreateRequest.Parent = accountTypeCreateRequest.Value;

        // Act
        Func<Task> act = async () =>
            await accountTypeService.CreateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeCreateSameNameAsParentError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenParentDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await accountTypeService.CreateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeCreateParentNotFoundError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenParentIsDefaultAccountType_ShouldNotThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();
        accountTypeCreateRequest.Parent = AccountTypeConstants
            .DefaultAccountTypes.Where(at => at.Parent.Length == 0)
            .First()
            .Value;

        // Act
        await accountTypeService.CreateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeCreateRequest
        );

        // Assert
        helper.UserDataContext.AccountTypes.Should().HaveCount(1);
        helper
            .UserDataContext.AccountTypes.Single()
            .Parent.Should()
            .Be(accountTypeCreateRequest.Parent);
    }

    [Fact]
    public async Task ReadAccountTypesAsync_WhenCalledWithValidData_ShouldReturnAccountTypes()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await accountTypeService.ReadAccountTypesAsync(helper.demoUser.Id);

        // Assert
        result.Select(r => r.ID).Should().Contain(accountTypes.Select(a => a.ID));
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithValidData_ShouldUpdateAccountType()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);
        accountTypes.ForEach(at =>
            at.Parent = AccountTypeConstants.DefaultAccountTypes.First().Value
        );

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountTypes.First().ID;
        accountTypeUpdateRequest.Parent = accountTypes.First().Parent;

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.First()
            .Value.Should()
            .Be(accountTypeUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenValueChanges_ShouldUpdateAccountsUsingThatType()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountType = accountTypeFaker.Generate();
        accountType.Parent = AccountTypeConstants.DefaultAccountTypes.First().Value;

        helper.UserDataContext.AccountTypes.Add(accountType);

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Type = accountType.Value;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountType.ID;
        accountTypeUpdateRequest.Parent = accountType.Parent;

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        account.Type.Should().Be(accountTypeUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithInvalidAccountTypeID_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeUpdateNotFoundError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithDuplicateName_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountTypes.First().ID;
        accountTypeUpdateRequest.Value = accountTypes.Last().Value;

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeUpdateDuplicateNameError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithEmptyName_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountTypes.First().ID;
        accountTypeUpdateRequest.Value = string.Empty;

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeUpdateEmptyNameError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithSameNameAsParent_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountTypes.First().ID;
        accountTypeUpdateRequest.Parent = accountTypeUpdateRequest.Value;

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeUpdateSameNameAsParentError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithParentThatDoesNotExist_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountTypes.First().ID;
        accountTypeUpdateRequest.Parent = "NonExistentParent";

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeUpdateParentNotFoundError");
    }

    [Fact]
    public async Task DeleteAccountTypeAsync_WhenCalledWithValidData_ShouldDeleteAccountType()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);
        for (var i = 1; i < accountTypes.Count; i++)
        {
            accountTypes[i].Parent = accountTypes.First().Value;
        }

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountTypeService.DeleteAccountTypeAsync(helper.demoUser.Id, accountTypes.Last().ID);

        // Assert
        helper.UserDataContext.AccountTypes.Should().NotContainEquivalentOf(accountTypes.Last());
    }

    [Fact]
    public async Task DeleteAccountTypeAsync_WhenCalledWithInvalidAccountTypeID_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountTypes = accountTypeFaker.Generate(5);

        helper.UserDataContext.AccountTypes.AddRange(accountTypes);
        helper.UserDataContext.SaveChanges();

        // Act
        Func<Task> act = async () =>
            await accountTypeService.DeleteAccountTypeAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeDeleteNotFoundError");
    }

    [Fact]
    public async Task DeleteAccountTypeAsync_WhenAccountTypeInUseByAccount_ShouldResetAccountType()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountType = accountTypeFaker.Generate();

        helper.UserDataContext.AccountTypes.Add(accountType);

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Type = accountType.Value;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountTypeService.DeleteAccountTypeAsync(helper.demoUser.Id, accountType.ID);

        // Assert
        helper.UserDataContext.AccountTypes.Should().NotContain(accountType);
        account.Type.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAccountTypeAsync_WhenAccountTypeHasChildren_ShouldDeleteChildrenAndResetAccounts()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var parentAccountType = accountTypeFaker.Generate();
        var childAccountType = accountTypeFaker.Generate();
        childAccountType.Parent = parentAccountType.Value;

        helper.UserDataContext.AccountTypes.AddRange([parentAccountType, childAccountType]);

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.Type = childAccountType.Value;

        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await accountTypeService.DeleteAccountTypeAsync(helper.demoUser.Id, parentAccountType.ID);

        // Assert
        helper.UserDataContext.AccountTypes.Should().NotContain(parentAccountType);
        helper.UserDataContext.AccountTypes.Should().NotContain(childAccountType);
        account.Type.Should().BeNull();
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenClassificationIsInvalid_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();
        accountTypeCreateRequest.Parent = string.Empty;
        accountTypeCreateRequest.Classification = "invalid";

        // Act
        Func<Task> act = async () =>
            await accountTypeService.CreateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeInvalidClassificationError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenParentExists_ShouldInheritParentClassification()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Use a known liability parent from defaults
        var liabilityParent = AccountTypeConstants.DefaultAccountTypes.First(at =>
            at.Classification == AccountClassifications.Liability && string.IsNullOrEmpty(at.Parent)
        );

        var accountTypeCreateRequest = _accountTypeCreateRequestFaker.Generate();
        accountTypeCreateRequest.Parent = liabilityParent.Value;
        accountTypeCreateRequest.Classification = AccountClassifications.Asset; // should be overridden

        // Act
        await accountTypeService.CreateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeCreateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.Single()
            .Classification.Should()
            .Be(AccountClassifications.Liability);
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenClassificationIsInvalid_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountType = accountTypeFaker.Generate();
        accountType.Parent = string.Empty;

        helper.UserDataContext.AccountTypes.Add(accountType);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountType.ID;
        accountTypeUpdateRequest.Parent = string.Empty;
        accountTypeUpdateRequest.Classification = "invalid";

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeInvalidClassificationError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenParentExists_ShouldInheritParentClassification()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var accountTypeFaker = new AccountTypeFaker(helper.demoUser.Id);
        var accountType = accountTypeFaker.Generate();
        accountType.Parent = AccountTypeConstants.DefaultAccountTypes.First().Value;

        helper.UserDataContext.AccountTypes.Add(accountType);
        helper.UserDataContext.SaveChanges();

        // Use a known liability parent from defaults
        var liabilityParent = AccountTypeConstants.DefaultAccountTypes.First(at =>
            at.Classification == AccountClassifications.Liability && string.IsNullOrEmpty(at.Parent)
        );

        var accountTypeUpdateRequest = _accountTypeUpdateRequestFaker.Generate();
        accountTypeUpdateRequest.ID = accountType.ID;
        accountTypeUpdateRequest.Parent = liabilityParent.Value;
        accountTypeUpdateRequest.Classification = AccountClassifications.Asset; // should be overridden

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.Single()
            .Classification.Should()
            .Be(AccountClassifications.Liability);
    }
}
