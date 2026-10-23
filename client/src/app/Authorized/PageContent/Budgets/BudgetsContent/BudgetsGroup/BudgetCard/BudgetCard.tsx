import { Card } from "@teelur/budget-board-ui";
import { IBudget } from "~/models/budget";
import { CategoryTypes, ICategoryNode } from "~/models/category";
import BudgetParentItem from "./BudgetParentItem/BudgetParentItem";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { getCategoryIcon } from "~/helpers/category";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import BudgetChildItem from "./BudgetChildItem/BudgetChildItem";
import { useTranslation } from "react-i18next";
import { areStringsEqual, roundAwayFromZero } from "~/helpers/utils";
import UnbudgetedChildItem from "./UnbudgetedChildItem/UnbudgetedChildItem";

interface BudgetParentCardProps {
  categoryNode: ICategoryNode;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToLimitsMap: Map<string, number>;
  categoryToRolloverMap: Map<string, number>;
  categoryToTransactionsTotalMap: Map<string, number>;
  categoryToRecurringForecastTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
  isCollapsed: boolean;
  toggleCollapsed: () => void;
}

const BudgetCard = (props: BudgetParentCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const { allTransactionCategories } = useTransactionCategories();

  const isIncome = areStringsEqual(
    props.categoryNode.categoryType,
    CategoryTypes.Income,
  );
  const budgets =
    props.categoryToBudgetsMap.get(props.categoryNode.value.toLowerCase()) ??
    [];
  const parentRollsOver = budgets.some(
    (budget) => budget.rolloverStartMonth !== null,
  );

  const buildChildren = (): [string, React.ReactNode][] => {
    const budgetedChildCards: [string, React.ReactNode][] = [];
    const unbudgetedChildCards: [string, React.ReactNode][] = [];

    props.categoryNode.subCategories.forEach((subCategory) => {
      if (
        props.categoryToBudgetsMap.has(subCategory.value.toLocaleLowerCase())
      ) {
        const budgets =
          props.categoryToBudgetsMap.get(
            subCategory.value.toLocaleLowerCase(),
          ) ?? [];
        const budget =
          props.selectedDate !== null && budgets.length === 1
            ? budgets[0]
            : null;
        budgetedChildCards.push([
          subCategory.value,
          <BudgetChildItem
            key={subCategory.value}
            id={budget?.id ?? ""}
            categoryValue={subCategory.value}
            amount={
              props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0
            }
            projectedAmount={
              (props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0) +
              (props.categoryToRecurringForecastTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0)
            }
            limit={
              props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ??
              0
            }
            rollover={
              props.categoryToRolloverMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0
            }
            rolloverStartMonth={budgets[0]?.rolloverStartMonth ?? null}
            rolloverDisabledReason={
              parentRollsOver
                ? t("rollover_on_for_category", {
                    category: props.categoryNode.value,
                  })
                : undefined
            }
            isIncome={isIncome}
            icon={getCategoryIcon(subCategory.value, allTransactionCategories)}
            selectedDate={props.selectedDate ?? dayjs().toDate()}
          />,
        ]);
      } else if (
        props.categoryToTransactionsTotalMap.has(
          subCategory.value.toLocaleLowerCase(),
        )
      ) {
        const amount =
          props.categoryToTransactionsTotalMap.get(
            subCategory.value.toLowerCase(),
          ) ?? 0;
        if (roundAwayFromZero(amount) !== 0) {
          unbudgetedChildCards.push([
            subCategory.value,
            <UnbudgetedChildItem
              key={subCategory.value}
              category={subCategory.value}
              amount={amount}
              selectedDate={props.selectedDate}
              isIncome={isIncome}
              icon={getCategoryIcon(
                subCategory.value,
                allTransactionCategories,
              )}
            />,
          ]);
        }
      }
    });

    return [...budgetedChildCards, ...unbudgetedChildCards];
  };

  return (
    <Card w="100%">
      <Card.Section
        onClick={() =>
          props.openDetails(props.categoryNode.value, props.selectedDate)
        }
        hoverable
      >
        <BudgetParentItem
          categoryNode={props.categoryNode}
          categoryToBudgetsMap={props.categoryToBudgetsMap}
          categoryToLimitsMap={props.categoryToLimitsMap}
          categoryToRolloverMap={props.categoryToRolloverMap}
          categoryToTransactionsTotalMap={props.categoryToTransactionsTotalMap}
          categoryToRecurringForecastTotalMap={
            props.categoryToRecurringForecastTotalMap
          }
          selectedDate={props.selectedDate}
          isCollapsed={props.isCollapsed}
          isIncome={isIncome}
          toggleCollapsed={props.toggleCollapsed}
          hasChildren={false}
        />
      </Card.Section>
      {buildChildren().map((child, index) => {
        return (
          <Card.Section
            key={index}
            onClick={() => props.openDetails(child[0], props.selectedDate)}
            hoverable
          >
            {child[1]}
          </Card.Section>
        );
      })}
    </Card>
  );
};

export default BudgetCard;
