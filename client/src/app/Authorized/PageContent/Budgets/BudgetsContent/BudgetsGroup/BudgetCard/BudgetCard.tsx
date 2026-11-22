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
  categoryToDirectTransactionsTotalMap: Map<string, number>;
  categoryToRecurringForecastTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (
    category: string,
    month: Date | null,
    directParentOnly?: boolean,
  ) => void;
  isCollapsed: boolean;
  toggleCollapsed: () => void;
}

interface BudgetCardChild {
  key: string;
  category: string;
  directParentOnly: boolean;
  node: React.ReactNode;
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

  const buildChildren = (): BudgetCardChild[] => {
    const budgetedChildCards: BudgetCardChild[] = [];
    const unbudgetedChildCards: BudgetCardChild[] = [];

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
        budgetedChildCards.push({
          key: subCategory.value,
          category: subCategory.value,
          directParentOnly: false,
          node: (
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
                props.categoryToLimitsMap.get(
                  subCategory.value.toLowerCase(),
                ) ?? 0
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
              icon={getCategoryIcon(
                subCategory.value,
                allTransactionCategories,
              )}
              selectedDate={props.selectedDate ?? dayjs().toDate()}
            />
          ),
        });
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
          unbudgetedChildCards.push({
            key: subCategory.value,
            category: subCategory.value,
            directParentOnly: false,
            node: (
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
              />
            ),
          });
        }
      }
    });

    const directAmount =
      props.categoryToDirectTransactionsTotalMap.get(
        props.categoryNode.value.toLocaleLowerCase(),
      ) ?? 0;
    if (roundAwayFromZero(directAmount) !== 0) {
      unbudgetedChildCards.push({
        key: `${props.categoryNode.value}-no-subcategory`,
        category: props.categoryNode.value,
        directParentOnly: true,
        node: (
          <UnbudgetedChildItem
            category={t("no_subcategory")}
            amount={directAmount}
            selectedDate={props.selectedDate}
            isIncome={isIncome}
            icon=""
            canCreateBudget={false}
          />
        ),
      });
    }

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
      {buildChildren().map((child) => {
        return (
          <Card.Section
            key={child.key}
            onClick={() =>
              props.openDetails(
                child.category,
                props.selectedDate,
                child.directParentOnly,
              )
            }
            hoverable
          >
            {child.node}
          </Card.Section>
        );
      })}
    </Card>
  );
};

export default BudgetCard;
