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

    #region CreateAccountTypeAsync
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
        var createdAccountType = helper.UserDataContext.AccountTypes.Single(at =>
            at.Value == accountTypeCreateRequest.Value
        );
        createdAccountType.Parent.Should().Be(accountTypeCreateRequest.Parent);
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenCreatingEmptyName_ShouldThrowAccountTypeEmptyNameError()
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
            .WithMessage("AccountTypeEmptyNameError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenCreatingDuplicate_ShouldThrowAccountTypeDuplicateNameError()
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
            .WithMessage("AccountTypeDuplicateNameError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenParentSameAsValue_ShouldThrowAccountTypeSameNameAsParentError()
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
            .WithMessage("AccountTypeSameNameAsParentError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenParentDoesNotExist_ShouldThrowAccountTypeParentNotFoundError()
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
            .WithMessage("AccountTypeParentNotFoundError");
    }

    [Fact]
    public async Task CreateAccountTypeAsync_WhenClassificationIsInvalid_ShouldThrowAccountTypeInvalidClassificationError()
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
            .DefaultAccountTypes.First(at => at.Parent.Length == 0)
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
    public async Task CreateAccountTypeAsync_WhenParentIsDifferentClassification_ShouldResolveClassification()
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
            .DefaultAccountTypes.First(at => at.Classification == AccountTypeClassification.Asset)
            .Value;
        accountTypeCreateRequest.Classification = AccountTypeClassification.Liability;

        // Act
        await accountTypeService.CreateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeCreateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.Single()
            .Classification.Should()
            .Be(AccountTypeClassification.Asset);
    }
    #endregion

    #region ReadAccountTypesAsync
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
    public async Task ReadAccountTypesAsync_WhenDefaultAccountTypesAreDisabled_ShouldNotReturnDefaultAccountTypes()
    {
        // Arrange
        var helper = new TestHelper();

        var accountTypeService = new AccountTypeService(
            Mock.Of<ILogger<IAccountTypeService>>(),
            helper.UserDataContext,
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        helper.UserDataContext.UserSettings.Add(
            new UserSettings { UserID = helper.demoUser.Id, DisableBuiltInAccountTypes = true }
        );
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await accountTypeService.ReadAccountTypesAsync(helper.demoUser.Id);

        // Assert
        result
            .Should()
            .NotContain(r =>
                AccountTypeConstants.DefaultAccountTypes.Any(dat => dat.Value == r.Value)
            );
    }
    #endregion

    #region UpdateAccountTypeAsync
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest
        {
            ID = accountTypes.First().ID,
            Parent = accountTypes.First().Parent,
            Value = "UpdatedValue",
        };

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
    public async Task UpdateAccountTypeAsync_WhenCalledWithInvalidAccountTypeID_ShouldThrowAccountTypeNotFoundError()
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = Guid.NewGuid(),
            Value = "test",
        };

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeNotFoundError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithEmptyName_ShouldThrowAccountTypeEmptyNameError()
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = accountTypes.First().ID,
            Parent = accountTypes.First().Parent,
            Value = string.Empty,
        };

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeEmptyNameError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithDuplicateName_ShouldThrowAccountTypeDuplicateNameError()
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = accountTypes.First().ID,
            Parent = accountTypes.First().Parent,
            Value = accountTypes.Last().Value,
        };

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeDuplicateNameError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithSameNameAsParent_ShouldThrowAccountTypeSameNameAsParentError()
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = accountTypes.First().ID,
            Parent = accountTypes.First().Value,
            Value = accountTypes.First().Value,
        };

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeSameNameAsParentError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithParentThatDoesNotExist_ShouldThrowAccountTypeParentNotFoundError()
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = accountTypes.First().ID,
            Parent = "NonExistentParent",
            Value = accountTypes.First().Value,
        };

        // Act
        Func<Task> act = async () =>
            await accountTypeService.UpdateAccountTypeAsync(
                helper.demoUser.Id,
                accountTypeUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("AccountTypeParentNotFoundError");
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenCalledWithInvalidClassification_ShouldThrowAccountTypeInvalidClassificationError()
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = accountType.ID,
            Parent = accountType.Parent,
            Value = accountType.Value,
            Classification = "InvalidClassification",
        };

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
    public async Task UpdateAccountTypeAsync_WhenParentIsDifferentClassification_ShouldResolveClassification()
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
        accountType.Classification = AccountTypeClassification.Liability;

        helper.UserDataContext.AccountTypes.Add(accountType);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = accountType.ID,
            Parent = AccountTypeConstants
                .DefaultAccountTypes.First(at =>
                    at.Classification == AccountTypeClassification.Asset
                )
                .Value,
            Value = accountType.Value,
            Classification = AccountTypeClassification.Liability,
        };

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.Single(at => at.ID == accountType.ID)
            .Classification.Should()
            .Be(AccountTypeClassification.Asset);
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

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = accountType.ID,
            Parent = accountType.Parent,
            Value = "UpdatedAccountTypeValue",
        };

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        account.Type.Should().Be(accountTypeUpdateRequest.Value);
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenParentIsChangedToChild_ShouldUpdateChildrenToBeParents()
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
        parentAccountType.Parent = string.Empty;

        var childAccountType = accountTypeFaker.Generate();
        childAccountType.Parent = parentAccountType.Value;

        var anotherParentAccountType = accountTypeFaker.Generate();
        anotherParentAccountType.Parent = string.Empty;

        helper.UserDataContext.AccountTypes.AddRange(
            [parentAccountType, childAccountType, anotherParentAccountType]
        );
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = parentAccountType.ID,
            Parent = anotherParentAccountType.Value,
            Value = parentAccountType.Value,
        };

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.Single(at => at.ID == parentAccountType.ID)
            .Parent.Should()
            .Be(anotherParentAccountType.Value);

        helper
            .UserDataContext.AccountTypes.Single(at => at.ID == childAccountType.ID)
            .Parent.Should()
            .Be(string.Empty);
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenUpdateClassification_ShouldAlsoUpdateChildrenClassification()
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
        parentAccountType.Parent = string.Empty;
        parentAccountType.Classification = AccountTypeClassification.Asset;

        var childAccountType = accountTypeFaker.Generate();
        childAccountType.Parent = parentAccountType.Value;
        childAccountType.Classification = AccountTypeClassification.Asset;

        helper.UserDataContext.AccountTypes.AddRange([parentAccountType, childAccountType]);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = parentAccountType.ID,
            Parent = parentAccountType.Parent,
            Value = parentAccountType.Value,
            Classification = AccountTypeClassification.Liability,
        };

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.Single(at => at.ID == parentAccountType.ID)
            .Classification.Should()
            .Be(AccountTypeClassification.Liability);

        helper
            .UserDataContext.AccountTypes.Single(at => at.ID == childAccountType.ID)
            .Classification.Should()
            .Be(AccountTypeClassification.Liability);
    }

    [Fact]
    public async Task UpdateAccountTypeAsync_WhenUpdateParentValue_ShouldUpdateChildrenParent()
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
        parentAccountType.Parent = string.Empty;

        var childAccountType = accountTypeFaker.Generate();
        childAccountType.Parent = parentAccountType.Value;

        helper.UserDataContext.AccountTypes.AddRange([parentAccountType, childAccountType]);
        helper.UserDataContext.SaveChanges();

        var accountTypeUpdateRequest = new AccountTypeUpdateRequest()
        {
            ID = parentAccountType.ID,
            Parent = parentAccountType.Parent,
            Value = "UpdatedParentValue",
        };

        // Act
        await accountTypeService.UpdateAccountTypeAsync(
            helper.demoUser.Id,
            accountTypeUpdateRequest
        );

        // Assert
        helper
            .UserDataContext.AccountTypes.Single(at => at.ID == childAccountType.ID)
            .Parent.Should()
            .Be(accountTypeUpdateRequest.Value);
    }
    #endregion

    #region DeleteAccountTypeAsync
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
    public async Task DeleteAccountTypeAsync_WhenCalledWithInvalidAccountTypeID_ShouldThrowAccountTypeNotFoundError()
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
            .WithMessage("AccountTypeNotFoundError");
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
        account.Type.Should().Be(string.Empty);
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
        account.Type.Should().Be(string.Empty);
    }
    #endregion
}
