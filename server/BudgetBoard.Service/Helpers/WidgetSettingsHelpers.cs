using BudgetBoard.Service.Models;

namespace BudgetBoard.Service.Helpers;

public static class WidgetSettingsHelpers
{
    public static readonly NetWorthWidgetConfiguration DefaultNetWorthWidgetConfiguration = new()
    {
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
                        Value = "Credit Card",
                        Type = "Account",
                        Subtype = "Category",
                    },
                ],
                Group = 0,
                Index = 0,
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
                Group = 0,
                Index = 1,
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
                Group = 0,
                Index = 1,
            },
            new NetWorthWidgetLine
            {
                Name = "Liquid",
                Categories =
                [
                    new NetWorthWidgetCategory { Value = "Spending", Type = "Line" },
                    new NetWorthWidgetCategory { Value = "Loans", Type = "Line" },
                    new NetWorthWidgetCategory { Value = "Savings", Type = "Line" },
                ],
                Group = 1,
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
                Group = 1,
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
                Group = 1,
                Index = 2,
            },
            new NetWorthWidgetLine()
            {
                Name = "Total",
                Categories =
                [
                    new NetWorthWidgetCategory { Value = "Liquid", Type = "Line" },
                    new NetWorthWidgetCategory { Value = "Investments", Type = "Line" },
                    new NetWorthWidgetCategory { Value = "Assets", Type = "Line" },
                ],
                Group = 2,
                Index = 0,
            },
        ],
    };
}
