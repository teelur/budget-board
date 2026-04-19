using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;

namespace BudgetBoard.Service.Helpers;

public record DefaultWidgetLayout(string WidgetType, int X, int Y, int W, int H);

public static class WidgetSettingsHelpers
{
    /// <summary>
    /// Default grid positions for each widget on a 12-column grid.
    /// Matches the original two-column dashboard layout.
    /// </summary>
    public static readonly IReadOnlyList<DefaultWidgetLayout> DefaultLayouts =
    [
        new DefaultWidgetLayout(WidgetTypes.Accounts, X: 0, Y: 0, W: 4, H: 5),
        new DefaultWidgetLayout(WidgetTypes.UncategorizedTransactions, X: 4, Y: 0, W: 8, H: 5),
        new DefaultWidgetLayout(WidgetTypes.NetWorth, X: 0, Y: 5, W: 4, H: 5),
        new DefaultWidgetLayout(WidgetTypes.SpendingTrends, X: 4, Y: 5, W: 8, H: 5),
    ];

    public static readonly NetWorthWidgetConfiguration DefaultNetWorthWidgetConfiguration = new()
    {
        Groups =
        [
            new NetWorthWidgetGroup
            {
                Index = 0,
                Lines =
                [
                    new NetWorthWidgetLine
                    {
                        Name = "Spending",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "Checking",
                                Type = "Account",
                                Subtype = "Category",
                            },
                            new NetWorthWidgetCategory
                            {
                                Value = "Cash",
                                Type = "Account",
                                Subtype = "Category",
                            },
                            new NetWorthWidgetCategory
                            {
                                Value = "Other",
                                Type = "Account",
                                Subtype = "Category",
                            },
                        ],
                        Index = 0,
                    },
                    new NetWorthWidgetLine
                    {
                        Name = "Credit Cards",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "Credit Card",
                                Type = "Account",
                                Subtype = "Category",
                            },
                        ],
                        Index = 1,
                    },
                    new NetWorthWidgetLine
                    {
                        Name = "Loans",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "Loan",
                                Type = "Account",
                                Subtype = "Category",
                            },
                        ],
                        Index = 2,
                    },
                    new NetWorthWidgetLine
                    {
                        Name = "Savings",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "Savings",
                                Type = "Account",
                                Subtype = "Category",
                            },
                        ],
                        Index = 3,
                    },
                ],
            },
            new NetWorthWidgetGroup
            {
                Index = 1,
                Lines =
                [
                    new NetWorthWidgetLine
                    {
                        Name = "Liquid",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "Spending",
                                Type = "Line",
                                Subtype = "Name",
                            },
                            new NetWorthWidgetCategory
                            {
                                Value = "Credit Cards",
                                Type = "Line",
                                Subtype = "Name",
                            },
                            new NetWorthWidgetCategory
                            {
                                Value = "Loans",
                                Type = "Line",
                                Subtype = "Name",
                            },
                            new NetWorthWidgetCategory
                            {
                                Value = "Savings",
                                Type = "Line",
                                Subtype = "Name",
                            },
                        ],
                        Index = 0,
                    },
                    new NetWorthWidgetLine()
                    {
                        Name = "Investments",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "Investment",
                                Type = "Account",
                                Subtype = "Category",
                            },
                        ],
                        Index = 1,
                    },
                    new NetWorthWidgetLine()
                    {
                        Name = "Assets",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "",
                                Type = "Asset",
                                Subtype = "All",
                            },
                        ],
                        Index = 2,
                    },
                ],
            },
            new NetWorthWidgetGroup
            {
                Index = 2,
                Lines =
                [
                    new NetWorthWidgetLine()
                    {
                        Name = "Total",
                        Categories =
                        [
                            new NetWorthWidgetCategory
                            {
                                Value = "Liquid",
                                Type = "Line",
                                Subtype = "Name",
                            },
                            new NetWorthWidgetCategory
                            {
                                Value = "Investments",
                                Type = "Line",
                                Subtype = "Name",
                            },
                            new NetWorthWidgetCategory
                            {
                                Value = "Assets",
                                Type = "Line",
                                Subtype = "Name",
                            },
                        ],
                        Index = 0,
                    },
                ],
            },
        ],
    };
}
