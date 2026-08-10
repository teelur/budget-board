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
public class TagServiceTests
{
    #region CreateTransactionAsync
    [Fact]
    public async Task CreateTransactionAsync_ShouldNormalizeAndReuseTags()
    {
        var helper = new TestHelper();
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();
        var tagService = CreateTagService(helper);
        var transactionService = CreateTransactionService(helper, tagService);

        await CreateTransactionWithTagsAsync(
            helper,
            transactionService,
            tagService,
            account,
            10,
            new DateOnly(2026, 8, 1),
            ["  Travel ", "travel", "Work"]
        );
        await CreateTransactionWithTagsAsync(
            helper,
            transactionService,
            tagService,
            account,
            20,
            new DateOnly(2026, 8, 2),
            ["TRAVEL"]
        );

        helper.UserDataContext.Tags.Should().HaveCount(2);
        helper
            .UserDataContext.Tags.Select(tag => tag.Value)
            .Should()
            .BeEquivalentTo(["Travel", "Work"]);
        helper.UserDataContext.TransactionTags.Should().HaveCount(3);
    }
    #endregion

    #region ApplyTagChangesAsync
    [Fact]
    public async Task ApplyTagChangesAsync_WhenTagIsNullOrEmpty_ShouldThrowTagValueEmptyError()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);

        Func<Task> act = async () =>
            await tagService.ApplyTagChangesAsync(
                helper.demoUser.Id,
                CreateTransaction(),
                [null!],
                null
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TagValueEmptyError");
    }

    [Fact]
    public async Task ApplyTagChangesAsync_WhenTagIsTooLong_ShouldThrowTagValueTooLongError()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);

        Func<Task> act = async () =>
            await tagService.ApplyTagChangesAsync(
                helper.demoUser.Id,
                CreateTransaction(),
                [new string('x', Tag.MaxValueLength + 1)],
                null
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TagValueTooLongError");
    }

    [Fact]
    public async Task ApplyTagChangesAsync_WhenTagIsAddedAndRemoved_ShouldThrowOverlapError()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);

        Func<Task> act = async () =>
            await tagService.ApplyTagChangesAsync(
                helper.demoUser.Id,
                CreateTransaction(),
                ["Travel"],
                [" travel "]
            );

        await act.Should()
            .ThrowAsync<BudgetBoardServiceException>()
            .WithMessage("TagAddRemoveOverlapError");
    }

    [Fact]
    public async Task ApplyTagChangesAsync_WhenTagAlreadyExistsOnTransaction_ShouldNotAddDuplicate()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();
        var transactionService = CreateTransactionService(helper, tagService);

        var transaction = await CreateTransactionWithTagsAsync(
            helper,
            transactionService,
            tagService,
            account,
            10,
            new DateOnly(2026, 8, 1),
            ["Existing"]
        );

        var removedTagIds = await tagService.ApplyTagChangesAsync(
            helper.demoUser.Id,
            transaction,
            [" existing "],
            null
        );

        removedTagIds.Should().BeEmpty();
        helper.UserDataContext.Tags.Should().ContainSingle();
        helper.UserDataContext.TransactionTags.Should().ContainSingle();
    }

    [Fact]
    public async Task ApplyTagChangesAsync_WhenTrackedLinkIsDeleted_ShouldIgnoreItAsUnsaved()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);
        var transaction = CreateTransaction();
        var existingTag = new Tag
        {
            UserID = helper.demoUser.Id,
            Value = "Existing",
            NormalizedValue = "EXISTING",
        };
        var existingLink = new TransactionTag
        {
            TransactionID = transaction.ID,
            TagID = existingTag.ID,
            Transaction = transaction,
            Tag = existingTag,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.Tags.Add(existingTag);
        helper.UserDataContext.TransactionTags.Add(existingLink);
        await helper.UserDataContext.SaveChangesAsync();
        helper.UserDataContext.ChangeTracker.Clear();

        transaction = helper.UserDataContext.Transactions.Single(t => t.ID == transaction.ID);
        existingLink = helper.UserDataContext.TransactionTags.Single();
        transaction.TransactionTags.Add(existingLink);
        helper.UserDataContext.Entry(existingLink).State = EntityState.Deleted;
        helper.UserDataContext.ChangeTracker.AutoDetectChangesEnabled = false;

        var removedTagIds = await tagService.ApplyTagChangesAsync(
            helper.demoUser.Id,
            transaction,
            null,
            null
        );

        removedTagIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyTagChangesAsync_WhenPendingLinksHaveDifferentKeys_ShouldCompareAllKeys()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);
        var transaction = CreateTransaction();
        var existingTag = new Tag
        {
            UserID = helper.demoUser.Id,
            Value = "Existing",
            NormalizedValue = "EXISTING",
        };
        var existingLink = new TransactionTag
        {
            TransactionID = transaction.ID,
            TagID = existingTag.ID,
            Transaction = transaction,
            Tag = existingTag,
        };
        helper.UserDataContext.Transactions.Add(transaction);
        helper.UserDataContext.Tags.Add(existingTag);
        helper.UserDataContext.TransactionTags.Add(existingLink);
        await helper.UserDataContext.SaveChangesAsync();
        helper.UserDataContext.ChangeTracker.Clear();

        transaction = helper.UserDataContext.Transactions.Single(t => t.ID == transaction.ID);
        existingLink = helper.UserDataContext.TransactionTags.Include(link => link.Tag).Single();
        transaction.TransactionTags.Add(existingLink);
        var pendingDifferentTransaction = new TransactionTag
        {
            TransactionID = Guid.NewGuid(),
            TagID = Guid.NewGuid(),
        };
        transaction.TransactionTags.Add(pendingDifferentTransaction);
        helper.UserDataContext.ChangeTracker.AutoDetectChangesEnabled = false;

        var removedTagIds = await tagService.ApplyTagChangesAsync(
            helper.demoUser.Id,
            transaction,
            null,
            null
        );

        removedTagIds.Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyTagChangesAsync_WhenPendingLinkHasNoTag_ShouldIgnoreItForRemoval()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);
        var transaction = CreateTransaction();
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        transaction.TransactionTags.Add(
            new TransactionTag
            {
                TransactionID = transaction.ID,
                TagID = Guid.NewGuid(),
                Transaction = transaction,
            }
        );

        var removedTagIds = await tagService.ApplyTagChangesAsync(
            Guid.NewGuid(),
            transaction,
            null,
            ["Missing"]
        );

        removedTagIds.Should().BeEmpty();
    }
    #endregion

    #region DeleteOrphanedTagsAsync
    [Fact]
    public async Task DeleteOrphanedTagsAsync_WhenTagIsStillUsed_ShouldKeepTag()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();
        var transactionService = CreateTransactionService(helper, tagService);

        await CreateTransactionWithTagsAsync(
            helper,
            transactionService,
            tagService,
            account,
            10,
            new DateOnly(2026, 8, 1),
            ["Active"]
        );
        var tag = helper.UserDataContext.Tags.Single();

        await tagService.DeleteOrphanedTagsAsync(helper.demoUser.Id, [tag.ID]);

        helper.UserDataContext.Tags.Should().ContainSingle();
        helper.UserDataContext.Tags.Single().ID.Should().Be(tag.ID);
    }
    #endregion

    #region ReadSuggestionsAsync
    [Fact]
    public async Task ReadSuggestionsAsync_WhenLimitIsNotPositive_ShouldUseDefaultLimit()
    {
        var helper = new TestHelper();
        var tagService = CreateTagService(helper);
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();
        var transactionService = CreateTransactionService(helper, tagService);

        await CreateTransactionWithTagsAsync(
            helper,
            transactionService,
            tagService,
            account,
            10,
            new DateOnly(2026, 8, 1),
            ["DefaultLimit"]
        );

        var suggestions = await tagService.ReadSuggestionsAsync(helper.demoUser.Id, null, 0);

        suggestions.Should().Equal("DefaultLimit");
    }
    #endregion

    [Fact]
    public async Task ReadSuggestionsAsync_ShouldOnlyReturnActiveUserTags()
    {
        var helper = new TestHelper();
        var account = new AccountFaker(helper.demoUser.Id).Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();
        var tagService = CreateTagService(helper);
        var transactionService = CreateTransactionService(helper, tagService);

        await CreateTransactionWithTagsAsync(
            helper,
            transactionService,
            tagService,
            account,
            10,
            new DateOnly(2026, 8, 1),
            ["Frequent", "Filtered"]
        );
        await CreateTransactionWithTagsAsync(
            helper,
            transactionService,
            tagService,
            account,
            20,
            new DateOnly(2026, 8, 2),
            ["Frequent"]
        );

        var suggestions = await tagService.ReadSuggestionsAsync(helper.demoUser.Id, "fre", 10);
        suggestions.Should().Equal("Frequent");

        var deletedTransaction = helper.UserDataContext.Transactions.First();
        await transactionService.DeleteTransactionsAsync(
            helper.demoUser.Id,
            [deletedTransaction.ID]
        );
        var remainingSuggestions = await tagService.ReadSuggestionsAsync(
            helper.demoUser.Id,
            null,
            10
        );
        remainingSuggestions.Should().Equal("Frequent");

        var remainingTransaction = helper.UserDataContext.Transactions.Single(t =>
            t.ID != deletedTransaction.ID
        );
        await transactionService.DeleteTransactionsAsync(
            helper.demoUser.Id,
            [remainingTransaction.ID]
        );
        (await tagService.ReadSuggestionsAsync(helper.demoUser.Id, null, 10)).Should().BeEmpty();
        helper.UserDataContext.Tags.Should().BeEmpty();
    }

    private static async Task<Transaction> CreateTransactionWithTagsAsync(
        TestHelper helper,
        TransactionService transactionService,
        TagService tagService,
        Account account,
        decimal amount,
        DateOnly date,
        IEnumerable<string> tags
    )
    {
        await transactionService.CreateTransactionAsync(
            helper.demoUser,
            new TransactionCreateRequest
            {
                Amount = amount,
                Date = date,
                AccountID = account.ID,
            }
        );

        var transaction = helper.UserDataContext.Transactions.Single(t =>
            t.AccountID == account.ID && t.Amount == amount && t.Date == date
        );
        await tagService.ApplyTagChangesAsync(helper.demoUser.Id, transaction, tags, null);
        await helper.UserDataContext.SaveChangesAsync();
        return transaction;
    }

    private static TagService CreateTagService(TestHelper helper) =>
        new(helper.UserDataContext, TestHelper.CreateMockLocalizer<ResponseStrings>());

    private static Transaction CreateTransaction() =>
        new()
        {
            Amount = 0,
            Date = new DateOnly(2026, 8, 1),
            Source = TransactionSource.Manual.ToString(),
            AccountID = Guid.NewGuid(),
        };

    private static TransactionService CreateTransactionService(
        TestHelper helper,
        ITagService tagService
    ) =>
        new(
            Mock.Of<ILogger<ITransactionService>>(),
            helper.UserDataContext,
            Mock.Of<INowProvider>(),
            Mock.Of<IAutomaticTransactionCategorizerService>(),
            TestHelper.CreateMockLocalizer<ResponseStrings>(),
            TestHelper.CreateMockLocalizer<LogStrings>(),
            tagService
        );
}
