using BudgetBoard.Service.Models;
using BudgetBoard.Service.Models.Widgets.MetricWidget;
using BudgetBoard.Service.Models.Widgets.NetWorthWidget;

namespace BudgetBoard.Service.Helpers;

public record DefaultWidgetLayout(
    string WidgetType,
    int LgX,
    int LgY,
    int LgW,
    int LgH,
    int SmY,
    int SmH
);

public static class WidgetSettingsHelpers
{
    /// <summary>
    /// Default grid positions for each widget on a 12-column grid.
    /// Matches the original two-column dashboard layout.
    /// </summary>
    public static readonly IReadOnlyList<DefaultWidgetLayout> DefaultLayouts =
    [
        new DefaultWidgetLayout(
            WidgetTypes.Accounts,
            LgX: 0,
            LgY: 0,
            LgW: 4,
            LgH: 11,
            SmY: 0,
            SmH: 20
        ),
        new DefaultWidgetLayout(
            WidgetTypes.UncategorizedTransactions,
            LgX: 4,
            LgY: 6,
            LgW: 8,
            LgH: 11,
            SmY: 36,
            SmH: 11
        ),
        new DefaultWidgetLayout(
            WidgetTypes.NetWorth,
            LgX: 0,
            LgY: 11,
            LgW: 4,
            LgH: 10,
            SmY: 20,
            SmH: 10
        ),
        new DefaultWidgetLayout(
            WidgetTypes.SpendingTrends,
            LgX: 4,
            LgY: 17,
            LgW: 8,
            LgH: 16,
            SmY: 47,
            SmH: 16
        ),
        new DefaultWidgetLayout(
            WidgetTypes.Metric,
            LgX: 4,
            LgY: 0,
            LgW: 2,
            LgH: 6,
            SmY: 30,
            SmH: 6
        ),
    ];

    /// <summary>
    /// Generic default layout used when no specific layout is found for a widget.
    /// </summary>
    public static readonly DefaultWidgetLayout GenericDefaultLayout = new(
        "generic",
        LgX: 0,
        LgY: 0,
        LgW: 4,
        LgH: 5,
        SmY: 0,
        SmH: 5
    );

    /// <summary>
    /// Default configuration for the NetWorth widget.
    /// </summary>
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

    /// <summary>
    /// The default configuration for a metric widget, representing this month's spending.
    /// </summary>
    public static readonly MetricWidgetConfiguration DefaultMetricWidgetConfiguration = new()
    {
        Title = "This Month's Spending",
        Value = "@transactions.sum(this_month, type=expense)",
        Label = "total expenses",
    };
}
