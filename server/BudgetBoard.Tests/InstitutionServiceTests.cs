using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using BudgetBoard.Service.Resources;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class InstitutionServiceTests
{
    #region CreateInstitutionAsync
    [Fact]
    public async Task CreateInstitutionAsync_WhenCalledWithValidData_ShouldCreateInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionCreateRequest = new InstitutionCreateRequest { Name = "Test Institution" };

        // Act
        await institutionService.CreateInstitutionAsync(
            helper.demoUser.Id,
            institutionCreateRequest
        );

        // Assert
        var addedInstitution = helper.UserDataContext.Institutions.Single();
        addedInstitution.Name.Should().Be(institutionCreateRequest.Name);
    }

    [Fact]
    public async Task CreateInstitutionAsync_InvalidUserId_ThrowsInvalidUserError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionCreateRequest = new InstitutionCreateRequest { Name = "Test Institution" };

        // Act
        Func<Task> act = async () =>
            await institutionService.CreateInstitutionAsync(
                Guid.NewGuid(),
                institutionCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateInstitutionAsync_EmptyName_ThrowsInstitutionEmptyNameError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionCreateRequest = new InstitutionCreateRequest { Name = string.Empty };

        // Act
        Func<Task> act = async () =>
            await institutionService.CreateInstitutionAsync(
                helper.demoUser.Id,
                institutionCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionEmptyNameError");
    }

    [Fact]
    public async Task CreateInstitutionAsync_DuplicateName_ThrowsInstitutionDuplicateNameError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var institutionCreateRequest = new InstitutionCreateRequest { Name = institution.Name };

        // Act
        Func<Task> act = async () =>
            await institutionService.CreateInstitutionAsync(
                helper.demoUser.Id,
                institutionCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionDuplicateNameError");
    }
    #endregion

    #region ReadInstitutionsAsync
    [Fact]
    public async Task ReadInstitutionsAsync_WhenCalledWithValidData_ShouldReturnInstitutions()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account1 = accountFaker.Generate();
        var account2 = accountFaker.Generate();

        account1.InstitutionID = institution.ID;
        account2.InstitutionID = institution.ID;

        institution.Accounts = [account1, account2];

        helper.UserDataContext.Accounts.AddRange([account1, account2]);
        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await institutionService.ReadInstitutionsAsync(helper.demoUser.Id);

        // Assert
        var readInstitution = result.Single();
        readInstitution.ID.Should().Be(institution.ID);
        readInstitution.Name.Should().Be(institution.Name);
        readInstitution.Deleted.Should().Be(institution.Deleted);
        readInstitution.Index.Should().Be(institution.Index);
        readInstitution.UserID.Should().Be(institution.UserID);
        readInstitution
            .Accounts.Should()
            .BeEquivalentTo(institution.Accounts.Select(a => new AccountResponse(a)));
    }
    #endregion

    #region UpdateInstitutionAsync
    [Fact]
    public async Task UpdateInstitutionAsync_WhenCalledWithValidData_ShouldUpdateInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var institutionUpdateRequest = new InstitutionUpdateRequest
        {
            ID = institution.ID,
            Name = "Updated Institution Name",
        };

        // Act
        await institutionService.UpdateInstitutionAsync(
            helper.demoUser.Id,
            institutionUpdateRequest
        );

        // Assert
        var updatedInstitution = helper.UserDataContext.Institutions.Single();
        updatedInstitution.Name.Should().Be(institutionUpdateRequest.Name);
    }

    [Fact]
    public async Task UpdateInstitutionAsync_InvalidInstitutionId_ThrowsInstitutionNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionUpdateRequest = new InstitutionUpdateRequest
        {
            ID = Guid.NewGuid(),
            Name = "Updated Institution Name",
        };

        // Act
        Func<Task> act = async () =>
            await institutionService.UpdateInstitutionAsync(
                helper.demoUser.Id,
                institutionUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionNotFoundError");
    }

    [Fact]
    public async Task UpdateInstitutionAsync_DuplicateName_ThrowsInstitutionDuplicateNameError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution1 = institutionFaker.Generate();
        var institution2 = institutionFaker.Generate();

        helper.UserDataContext.Institutions.AddRange([institution1, institution2]);
        helper.UserDataContext.SaveChanges();

        var institutionUpdateRequest = new InstitutionUpdateRequest
        {
            ID = institution2.ID,
            Name = institution1.Name,
        };

        // Act
        Func<Task> act = async () =>
            await institutionService.UpdateInstitutionAsync(
                helper.demoUser.Id,
                institutionUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionDuplicateNameError");
    }

    [Fact]
    public async Task UpdateInstitutionAsync_EmptyName_ShouldThrowInstitutionEmptyNameError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var institutionUpdateRequest = new InstitutionUpdateRequest
        {
            ID = institution.ID,
            Name = string.Empty,
        };

        // Act
        Func<Task> act = async () =>
            await institutionService.UpdateInstitutionAsync(
                helper.demoUser.Id,
                institutionUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionEmptyNameError");
    }
    #endregion

    #region DeleteInstitutionAsync
    [Fact]
    public async Task DeleteInstitutionAsync_WhenCalledWithValidData_ShouldDeleteInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var insitutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = insitutionFaker.Generate();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.InstitutionID = institution.ID;
        account.Deleted = DateTime.UtcNow;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(10);

        account.Transactions = transactions;
        institution.Accounts.Add(account);

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await institutionService.DeleteInstitutionAsync(helper.demoUser.Id, institution.ID, false);

        // Assert
        helper.UserDataContext.Institutions.Single().Deleted.Should().NotBeNull();
        helper.UserDataContext.Institutions.Single().Index.Should().Be(0);
        helper.UserDataContext.Accounts.Single().Deleted.Should().NotBeNull();
        helper
            .UserDataContext.Transactions.Select(t => t.Deleted)
            .All(t => t == null)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task DeleteInstitutionAsync_WhenCalledWithDeleteTransactions_ShouldDeleteTransactions()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        account.InstitutionID = institution.ID;

        var transactionFaker = new TransactionFaker([account.ID]);
        var transactions = transactionFaker.Generate(10);

        account.Transactions = transactions;
        institution.Accounts.Add(account);

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await institutionService.DeleteInstitutionAsync(helper.demoUser.Id, institution.ID, true);

        // Assert
        helper.UserDataContext.Institutions.Single().Deleted.Should().NotBeNull();
        helper.UserDataContext.Accounts.Single().Deleted.Should().NotBeNull();
        helper.UserDataContext.Transactions.All(t => t.Deleted != null).Should().BeTrue();
    }

    [Fact]
    public async Task DeleteInstitutionAsync_WhenCalledWithDeferSave_ShouldNotSaveChanges()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        // Act
        await institutionService.DeleteInstitutionAsync(
            helper.demoUser.Id,
            institution.ID,
            false,
            deferSave: true
        );

        // Assert
        helper.UserDataContext.Institutions.Single().Deleted.Should().NotBeNull();
        helper.UserDataContext.Entry(institution).State.Should().Be(EntityState.Modified);
    }

    [Fact]
    public async Task DeleteInstitutionAsync_InvalidInstitutionId_ThrowsInstitutionNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        // Act
        Func<Task> act = async () =>
            await institutionService.DeleteInstitutionAsync(
                helper.demoUser.Id,
                Guid.NewGuid(),
                false
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionNotFoundError");
    }
    #endregion

    #region OrderInstitutionsAsync
    [Fact]
    public async Task OrderInstitutionsAsync_WhenCalledWithValidData_ShouldOrderInstitutions()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institutions = institutionFaker.Generate(10);

        var rnd = new Random();
        institutions = [.. institutions.OrderBy(i => rnd.Next())];
        institutions.ForEach(i => i.Index = institutions.IndexOf(i));

        helper.UserDataContext.Institutions.AddRange(institutions);
        helper.UserDataContext.SaveChanges();

        var orderedInstitutions = institutions
            .OrderBy(i => rnd.Next())
            .Select(i => new InstitutionIndexRequest { ID = i.ID, Index = institutions.IndexOf(i) })
            .ToList();

        // Act
        await institutionService.OrderInstitutionsAsync(helper.demoUser.Id, orderedInstitutions);

        // Assert
        helper
            .UserDataContext.Institutions.OrderBy(i => i.Index)
            .Select(i => i.ID)
            .Should()
            .BeEquivalentTo(orderedInstitutions.OrderBy(i => i.Index).Select(i => i.ID));
    }

    [Fact]
    public async Task OrderInstitutionsAsync_InvalidInstitutionId_ThrowsInstitutionNotFoundError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institutions = institutionFaker.Generate(10);

        var rnd = new Random();
        institutions = [.. institutions.OrderBy(i => rnd.Next())];
        institutions.ForEach(i => i.Index = institutions.IndexOf(i));

        helper.UserDataContext.Institutions.AddRange(institutions);
        helper.UserDataContext.SaveChanges();

        var orderedInstitutions = institutions
            .OrderBy(i => rnd.Next())
            .Select(i => new InstitutionIndexRequest { ID = i.ID, Index = institutions.IndexOf(i) })
            .ToList();
        orderedInstitutions.First().ID = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await institutionService.OrderInstitutionsAsync(
                helper.demoUser.Id,
                orderedInstitutions
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionNotFoundError");
    }
    #endregion
}
