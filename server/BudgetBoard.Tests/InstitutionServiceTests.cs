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

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class InstitutionServiceTests
{
    private readonly Faker<InstitutionCreateRequest> _institutionCreateRequestFaker =
        new Faker<InstitutionCreateRequest>().RuleFor(i => i.Name, f => f.Company.CompanyName());

    private readonly Faker<InstitutionUpdateRequest> _institutionUpdateRequestFaker =
        new Faker<InstitutionUpdateRequest>().RuleFor(i => i.Name, f => f.Company.CompanyName());

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
    public async Task CreateInstitutionAsync_InvalidUserId_ThrowsError()
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
            .WithMessage("InvalidUserError");
    }

    [Fact]
    public async Task CreateInstitutionAsync_DuplicateName_ThrowsError()
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

        var institutionCreateRequest = _institutionCreateRequestFaker.Generate();
        institutionCreateRequest.Name = institution.Name;

        // Act
        Func<Task> act = async () =>
            await institutionService.CreateInstitutionAsync(
                helper.demoUser.Id,
                institutionCreateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionCreateDuplicateNameError");
    }

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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionFaker = new InstitutionFaker(helper.demoUser.Id);
        var institution = institutionFaker.Generate();

        helper.UserDataContext.Institutions.Add(institution);
        helper.UserDataContext.SaveChanges();

        // Act
        var act = async () =>
            await institutionService.ReadInstitutionsAsync(helper.demoUser.Id, Guid.NewGuid());

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionNotFoundError");
    }

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

        var institutionUpdateRequest = _institutionUpdateRequestFaker.Generate();
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
            Mock.Of<INowProvider>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>()
        );

        var institutionUpdateRequest = _institutionUpdateRequestFaker.Generate();

        // Act
        Func<Task> act = async () =>
            await institutionService.UpdateInstitutionAsync(
                helper.demoUser.Id,
                institutionUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionUpdateNotFoundError");
    }

    [Fact]
    public async Task UpdateInstitutionAsync_DuplicateName_ThrowsError()
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

        var institutionUpdateRequest = _institutionUpdateRequestFaker.Generate();
        institutionUpdateRequest.ID = institution2.ID;
        institutionUpdateRequest.Name = institution1.Name;

        // Act
        Func<Task> act = async () =>
            await institutionService.UpdateInstitutionAsync(
                helper.demoUser.Id,
                institutionUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionUpdateDuplicateNameError");
    }

    [Fact]
    public async Task UpdateInstitutionAsync_EmptyName_ShouldThrowError()
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

        var institutionUpdateRequest = _institutionUpdateRequestFaker.Generate();
        institutionUpdateRequest.ID = institution.ID;
        institutionUpdateRequest.Name = string.Empty;

        // Act
        Func<Task> act = async () =>
            await institutionService.UpdateInstitutionAsync(
                helper.demoUser.Id,
                institutionUpdateRequest
            );

        // Assert
        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("InstitutionUpdateEmptyNameError");
    }

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
        helper.UserDataContext.Accounts.Single().Deleted.Should().NotBeNull();
        helper
            .UserDataContext.Transactions.Select(t => t.Deleted)
            .All(t => t == null)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task DeleteInsitutionAsync_WhenCalledWithDeleteTransactions_ShouldDeleteTransactions()
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
    public async Task DeleteInstitutionAsync_InvalidInstitutionId_ThrowsError()
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
            .WithMessage("InstitutionDeleteNotFoundError");
    }

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
    public async Task OrderInstitutionsAsync_InvalidInstitutionId_ThrowsError()
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
            .WithMessage("InstitutionOrderNotFoundError");
    }
}
