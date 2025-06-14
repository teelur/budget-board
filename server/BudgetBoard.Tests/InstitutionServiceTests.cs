using Bogus;
using BudgetBoard.IntegrationTests.Fakers;
using BudgetBoard.Service;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Interfaces;
using BudgetBoard.Service.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class InstitutionServiceTests
{
    private readonly Faker<InstitutionCreateRequest> _institutionCreateRequestFaker =
        new Faker<InstitutionCreateRequest>().RuleFor(i => i.Name, f => f.Company.CompanyName());

    private readonly Faker<InstitutionUpdateRequest> _institutionUpdateRequestFaker =
        new Faker<InstitutionUpdateRequest>().RuleFor(i => i.Name, f => f.Company.CompanyName());

    [Fact]
    public async Task CreateInstitutionAsync_InvalidUserId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionCreateRequest = _institutionCreateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await institutionService.CreateInstitutionAsync(
                Guid.NewGuid(),
                institutionCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("Provided user not found.");
    }

    [Fact]
    public async Task CreateInstitutionAsync_WhenCalledWithValidData_ShouldCreateInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionCreateRequest = _institutionCreateRequestFaker.Generate();

        // Act
        await institutionService.CreateInstitutionAsync(
            helper.demoUser.Id,
            institutionCreateRequest
        );

        // Assert
        helper.UserDataContext.Institutions.Should().ContainSingle();
        helper
            .UserDataContext.Institutions.Single()
            .Should()
            .BeEquivalentTo(institutionCreateRequest);
    }

    [Fact]
    public async Task ReadInstitutionsAsync_WhenCalledWithValidData_ShouldReturnInstitutions()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await institutionService.ReadInstitutionsAsync(helper.demoUser.Id);

        // Assert
        result.Should().ContainSingle();
        result.Single().Should().BeEquivalentTo(new InstitutionResponse(institution));
    }

    [Fact]
    public async Task ReadInstitutionsAsync_WhenCalledWithValidDataAndGuid_ShouldReturnInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        // Act
        var result = await institutionService.ReadInstitutionsAsync(
            helper.demoUser.Id,
            institution.ID
        );

        // Assert
        result.Should().ContainSingle();
        result.Single().Should().BeEquivalentTo(new InstitutionResponse(institution));
    }

    [Fact]
    public async Task ReadInsitutionsAsync_WhenInvalidGuid_ShouldThrowError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionFaker = new InstitutionFaker();
        var institution = institutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        // Act
        var act = async () =>
            await institutionService.ReadInstitutionsAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The institution you are trying to access does not exist.");
    }

    [Fact]
    public async Task UpdateInstitutionAsync_WhenCalledWithValidData_ShouldUpdateInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var insitutionFaker = new InstitutionFaker();
        var institution = insitutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        var institutionUpdateRequest = _institutionUpdateRequestFaker.Generate();
        institutionUpdateRequest.UserID = helper.demoUser.Id;
        institutionUpdateRequest.ID = institution.ID;

        // Act
        await institutionService.UpdateInstitutionAsync(
            helper.demoUser.Id,
            institutionUpdateRequest
        );

        // Assert
        helper.UserDataContext.Institutions.Should().ContainSingle();
        helper
            .UserDataContext.Institutions.Single()
            .Should()
            .BeEquivalentTo(institutionUpdateRequest);
    }

    [Fact]
    public async Task UpdateInstitutionAsync_InvalidInstitutionId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionUpdateRequest = _institutionUpdateRequestFaker.Generate();
        institutionUpdateRequest.UserID = helper.demoUser.Id;

        // Act
        Func<Task> act = async () =>
            await institutionService.UpdateInstitutionAsync(
                helper.demoUser.Id,
                institutionUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("The institution you are trying to update does not exist.");
    }

    [Fact]
    public async Task DeleteInstitutionAsync_WhenCalledWithValidData_ShouldDeleteInstitution()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var insitutionFaker = new InstitutionFaker();
        var institution = insitutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.InstitutionID = institution.ID;

        var transactionFaker = new TransactionFaker();
        transactionFaker.AccountIds.Add(account.ID);
        var transactions = transactionFaker.Generate(10);

        account.Transactions = transactions;
        institution.Accounts.Add(account);

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await institutionService.DeleteInstitutionAsync(helper.demoUser.Id, institution.ID, false);

        // Assert
        helper.UserDataContext.Institutions.Should().BeEmpty();
        helper
            .UserDataContext.Transactions.Select(t => t.Deleted)
            .Should()
            .AllBeEquivalentTo(default(DateTime?));
    }

    [Fact]
    public async Task DeleteInsitutionAsync_WhenCalledWithDeleteTransactions_ShouldDeleteTransactions()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var insitutionFaker = new InstitutionFaker();
        var institution = insitutionFaker.Generate();
        institution.UserID = helper.demoUser.Id;

        var accountFaker = new AccountFaker();
        var account = accountFaker.Generate();
        account.UserID = helper.demoUser.Id;
        account.InstitutionID = institution.ID;

        var transactionFaker = new TransactionFaker();
        transactionFaker.AccountIds.Add(account.ID);
        var transactions = transactionFaker.Generate(10);

        account.Transactions = transactions;
        institution.Accounts.Add(account);

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.Accounts.Add(account);
        helper.UserDataContext.SaveChanges();

        // Act
        await institutionService.DeleteInstitutionAsync(helper.demoUser.Id, institution.ID, true);

        // Assert
        helper.UserDataContext.Institutions.Should().BeEmpty();
        helper.UserDataContext.Transactions.Select(t => t.Deleted).Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteInstitutionAsync_InvalidInstitutionId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var updateInstitutionRequest = _institutionUpdateRequestFaker.Generate();
        updateInstitutionRequest.UserID = helper.demoUser.Id;

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
            .WithMessage("The institution you are trying to delete does not exist.");
    }

    [Fact]
    public async Task OrderInstitutionsAsync_WhenCalledWithValidData_ShouldOrderInstitutions()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionFaker = new InstitutionFaker();
        var institutions = institutionFaker.Generate(10);

        institutions.ForEach(i => i.UserID = helper.demoUser.Id);

        helper.UserDataContext.Institutions.AddRange(institutions);
        helper.UserDataContext.SaveChanges();

        List<IInstitutionIndexRequest> orderedInstitutions = [];
        foreach (var institution in institutions)
        {
            orderedInstitutions.Add(
                new InstitutionIndexRequest
                {
                    ID = institution.ID,
                    Index = institutions.IndexOf(institution),
                }
            );
        }

        // Act
        await institutionService.OrderInstitutionsAsync(helper.demoUser.Id, orderedInstitutions);

        // Assert
        foreach (var institution in institutions)
        {
            helper
                .UserDataContext.Institutions.Single(i => i.ID == institution.ID)
                .Should()
                .BeEquivalentTo(institution);
        }
    }

    [Fact]
    public async Task OrderInstitutionsAsync_InvalidInstitutionId_ThrowsError()
    {
        // Arrange
        var helper = new TestHelper();
        var institutionService = new InstitutionService(
            Mock.Of<ILogger<IInstitutionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>()
        );

        var institutionFaker = new InstitutionFaker();
        var institutions = institutionFaker.Generate(10);
        institutions.ForEach(i => i.UserID = helper.demoUser.Id);

        helper.UserDataContext.Institutions.AddRange(institutions);
        helper.UserDataContext.SaveChanges();

        List<IInstitutionIndexRequest> orderedInstitutions = [];
        foreach (var institution in institutions)
        {
            orderedInstitutions.Add(
                new InstitutionIndexRequest
                {
                    ID = institution.ID,
                    Index = institutions.IndexOf(institution),
                }
            );
        }
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
            .WithMessage("The institution you are trying to order does not exist.");
    }
}
