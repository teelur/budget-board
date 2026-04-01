using Bogus;
using BudgetBoard.Database.Models;
using BudgetBoard.IntegrationTests.Fakers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BudgetBoard.IntegrationTests;

[Collection("IntegrationTests")]
public class StringSanitizationInterceptorTests
{
    [Fact]
    public async Task SaveChanges_ShouldRemoveNullBytesFromTransactionStrings()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var transaction = new Transaction
        {
            SyncID = "sync\0id",
            Amount = 100.00M,
            Date = DateTime.UtcNow,
            Category = "Gro\0ceries",
            Subcategory = "Food\0Store",
            MerchantName = "Mer\0chant\0Name",
            Source = "Man\0ual",
            AccountID = account.ID,
        };

        // Act
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        // Assert
        var savedTransaction = await helper.UserDataContext.Transactions.FirstOrDefaultAsync(t =>
            t.ID == transaction.ID
        );

        savedTransaction.Should().NotBeNull();
        savedTransaction!.SyncID.Should().Be("syncid");
        savedTransaction.Category.Should().Be("Groceries");
        savedTransaction.Subcategory.Should().Be("FoodStore");
        savedTransaction.MerchantName.Should().Be("MerchantName");
        savedTransaction.Source.Should().Be("Manual");
    }

    [Fact]
    public async Task SaveChanges_ShouldRemoveNullBytesFromAccountStrings()
    {
        // Arrange
        var helper = new TestHelper();

        var account = new Account
        {
            Name = "Test\0Account",
            Type = "Check\0ing",
            Subtype = "Sub\0type",
            Source = "Man\0ual",
            UserID = helper.demoUser.Id,
        };

        // Act
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        // Assert
        var savedAccount = await helper.UserDataContext.Accounts.FirstOrDefaultAsync(a =>
            a.ID == account.ID
        );

        savedAccount.Should().NotBeNull();
        savedAccount!.Name.Should().Be("TestAccount");
        savedAccount.Type.Should().Be("Checking");
        savedAccount.Subtype.Should().Be("Subtype");
        savedAccount.Source.Should().Be("Manual");
    }

    [Fact]
    public async Task SaveChanges_ShouldRemoveNullBytesFromInstitutionStrings()
    {
        // Arrange
        var helper = new TestHelper();

        var institution = new Institution
        {
            Name = "Test\0Bank\0Name",
            UserID = helper.demoUser.Id,
        };

        // Act
        helper.UserDataContext.Institutions.Add(institution);
        await helper.UserDataContext.SaveChangesAsync();

        // Assert
        var savedInstitution = await helper.UserDataContext.Institutions.FirstOrDefaultAsync(i =>
            i.ID == institution.ID
        );

        savedInstitution.Should().NotBeNull();
        savedInstitution!.Name.Should().Be("TestBankName");
    }

    [Fact]
    public async Task SaveChanges_ShouldHandleMultipleNullBytes()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var transaction = new Transaction
        {
            SyncID = "syn\0c\0id",
            Amount = 100.00M,
            Date = DateTime.UtcNow,
            MerchantName = "Mer\0\0\0chant",
            Source = "Manual",
            AccountID = account.ID,
        };

        // Act
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        // Assert
        var savedTransaction = await helper.UserDataContext.Transactions.FirstOrDefaultAsync(t =>
            t.ID == transaction.ID
        );

        savedTransaction.Should().NotBeNull();
        savedTransaction!.SyncID.Should().Be("syncid");
        savedTransaction.MerchantName.Should().Be("Merchant");
    }

    [Fact]
    public async Task SaveChanges_ShouldHandleNullStrings()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var transaction = new Transaction
        {
            SyncID = null,
            Amount = 100.00M,
            Date = DateTime.UtcNow,
            Category = null,
            Subcategory = null,
            MerchantName = null,
            Source = "Manual",
            AccountID = account.ID,
        };

        // Act
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        // Assert
        var savedTransaction = await helper.UserDataContext.Transactions.FirstOrDefaultAsync(t =>
            t.ID == transaction.ID
        );

        savedTransaction.Should().NotBeNull();
        savedTransaction!.SyncID.Should().BeNull();
        savedTransaction.Category.Should().BeNull();
        savedTransaction.Subcategory.Should().BeNull();
        savedTransaction.MerchantName.Should().BeNull();
    }

    [Fact]
    public async Task SaveChanges_ShouldRemoveNullBytesOnUpdate()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var transaction = new Transaction
        {
            SyncID = "syncid",
            Amount = 100.00M,
            Date = DateTime.UtcNow,
            MerchantName = "Clean Merchant",
            Source = "Manual",
            AccountID = account.ID,
        };

        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        // Act - Update with null bytes
        transaction.MerchantName = "Dirty\0Mer\0chant";
        transaction.Category = "Gro\0ceries";
        await helper.UserDataContext.SaveChangesAsync();

        // Assert
        var savedTransaction = await helper.UserDataContext.Transactions.FirstOrDefaultAsync(t =>
            t.ID == transaction.ID
        );

        savedTransaction.Should().NotBeNull();
        savedTransaction!.MerchantName.Should().Be("DirtyMerchant");
        savedTransaction.Category.Should().Be("Groceries");
    }

    [Fact]
    public async Task SaveChanges_ShouldHandleEmptyStrings()
    {
        // Arrange
        var helper = new TestHelper();

        var accountFaker = new AccountFaker(helper.demoUser.Id);
        var account = accountFaker.Generate();
        helper.UserDataContext.Accounts.Add(account);
        await helper.UserDataContext.SaveChangesAsync();

        var transaction = new Transaction
        {
            SyncID = string.Empty,
            Amount = 100.00M,
            Date = DateTime.UtcNow,
            MerchantName = string.Empty,
            Source = "Manual",
            AccountID = account.ID,
        };

        // Act
        helper.UserDataContext.Transactions.Add(transaction);
        await helper.UserDataContext.SaveChangesAsync();

        // Assert
        var savedTransaction = await helper.UserDataContext.Transactions.FirstOrDefaultAsync(t =>
            t.ID == transaction.ID
        );

        savedTransaction.Should().NotBeNull();
        savedTransaction!.SyncID.Should().Be(string.Empty);
        savedTransaction.MerchantName.Should().Be(string.Empty);
    }
}
