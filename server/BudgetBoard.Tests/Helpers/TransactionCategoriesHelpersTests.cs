using BudgetBoard.Database.Models;
using BudgetBoard.Service.Helpers;
using BudgetBoard.Service.Models;
using FluentAssertions;

namespace BudgetBoard.IntegrationTests.Helpers;

[Collection("IntegrationTests")]
public class TransactionCategoriesHelpersTests
{
    private static readonly IReadOnlyList<ITransactionCategory> Categories =
    [
        new TransactionCategoryBase
        {
            Value = "Parent",
            Parent = string.Empty,
            CategoryType = TransactionCategoryTypes.Expense,
        },
        new TransactionCategoryBase
        {
            Value = "Child",
            Parent = "Parent",
            CategoryType = TransactionCategoryTypes.Expense,
        },
        new TransactionCategoryBase
        {
            Value = "Leaf",
            Parent = string.Empty,
            CategoryType = TransactionCategoryTypes.Income,
        },
    ];

    #region GetParentCategory
    [Fact]
    public void GetParentCategory_WhenCategoryIsTopLevel_ReturnsItsOwnValue()
    {
        var result = TransactionCategoriesHelpers.GetParentCategory("Parent", Categories);

        result.Should().Be("Parent");
    }

    [Fact]
    public void GetParentCategory_WhenCategoryHasParent_ReturnsParentValueCaseInsensitively()
    {
        var result = TransactionCategoriesHelpers.GetParentCategory("child", Categories);

        result.Should().Be("Parent");
    }

    [Fact]
    public void GetParentCategory_WhenCategoryIsUnknown_ReturnsEmptyString()
    {
        var result = TransactionCategoriesHelpers.GetParentCategory("Unknown", Categories);

        result.Should().BeEmpty();
    }
    #endregion

    #region GetIsParentCategory
    [Fact]
    public void GetIsParentCategory_WhenCategoryIsEmpty_ReturnsTrue()
    {
        var result = TransactionCategoriesHelpers.GetIsParentCategory(string.Empty, Categories);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetIsParentCategory_WhenCategoryHasChildren_ReturnsTrue()
    {
        var result = TransactionCategoriesHelpers.GetIsParentCategory("Parent", Categories);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetIsParentCategory_WhenCategoryHasNoChildren_ReturnsFalse()
    {
        var result = TransactionCategoriesHelpers.GetIsParentCategory("Leaf", Categories);

        result.Should().BeFalse();
    }

    [Fact]
    public void GetIsParentCategory_WhenCategoryIsUnknown_ReturnsFalse()
    {
        var result = TransactionCategoriesHelpers.GetIsParentCategory("Unknown", Categories);

        result.Should().BeFalse();
    }
    #endregion

    #region GetFullCategory
    [Fact]
    public void GetFullCategory_WhenCategoryIsParent_ReturnsParentAndEmptyChild()
    {
        var result = TransactionCategoriesHelpers.GetFullCategory("Parent", Categories);

        result.Should().Be(("Parent", string.Empty));
    }

    [Fact]
    public void GetFullCategory_WhenCategoryIsChild_ReturnsParentAndChild()
    {
        var result = TransactionCategoriesHelpers.GetFullCategory("Child", Categories);

        result.Should().Be(("Parent", "Child"));
    }

    [Fact]
    public void GetFullCategory_WhenCategoryIsUnknown_ReturnsUnknownAsChild()
    {
        var result = TransactionCategoriesHelpers.GetFullCategory("Unknown", Categories);

        result.Should().Be((string.Empty, "Unknown"));
    }
    #endregion

    #region GetAllTransactionCategories
    [Fact]
    public void GetAllTransactionCategories_WhenBuiltInsAreEnabled_IncludesCustomSpecialAndDefaultCategories()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        user.TransactionCategories.Add(
            new Category
            {
                Value = "Custom",
                Parent = string.Empty,
                CategoryType = TransactionCategoryTypes.Income,
                UserID = user.Id,
            }
        );

        var result = TransactionCategoriesHelpers.GetAllTransactionCategories(user);

        result.Should().Contain(category => category.Value == "Custom");
        result
            .Should()
            .Contain(category =>
                category.Value == TransactionCategoriesConstants.HideFromBudgetsCategory
            );
        result
            .Should()
            .Contain(category =>
                category.Value
                == TransactionCategoriesConstants.DefaultTransactionCategories.First().Value
            );
    }

    [Fact]
    public void GetAllTransactionCategories_WhenBuiltInsAreDisabled_ExcludesDefaultCategories()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid() };
        user.UserSettings = new UserSettings
        {
            UserID = user.Id,
            DisableBuiltInTransactionCategories = true,
        };

        var result = TransactionCategoriesHelpers.GetAllTransactionCategories(user);

        result
            .Should()
            .Contain(category =>
                category.Value == TransactionCategoriesConstants.HideFromBudgetsCategory
            );
        result
            .Should()
            .NotContain(category =>
                category.Value
                == TransactionCategoriesConstants.DefaultTransactionCategories.First().Value
            );
    }
    #endregion
}
