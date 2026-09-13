import { Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import MonthlySpendingChart from "~/components/Charts/MonthlySpendingChart/MonthlySpendingChart";
import { getCategoryIcon, getIsParentCategory } from "~/helpers/category";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { areStringsEqual } from "~/helpers/utils";
import TransactionCards from "./TransactionCards/TransactionCards";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { CategoryTypes } from "~/models/category";
import { useRecurringForecastQuery } from "~/hooks/queries/useRecurringForecastQuery";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import StatusText from "~/components/core/Text/StatusText/StatusText";

interface BudgetDetailsProps {
  isOpen: boolean;
  close: () => void;
  category: string | null;
  month: Date | null;
}

const BudgetDetails = (props: BudgetDetailsProps): React.ReactNode => {
  const chartLookbackMonths = 6;

  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const { allTransactionCategories, getCategoryType } =
    useTransactionCategories();
  const transactionsQuery = useTransactionsQuery();
  const forecastQuery = useRecurringForecastQuery({
    month: props.month,
    enabled: props.isOpen && props.month !== null,
  });

  const categoryIcon = getCategoryIcon(
    props.category ?? "",
    allTransactionCategories,
  );

  const transactionsForCategory = (transactionsQuery.data ?? [])
    .filter((transaction) =>
      dayjs(transaction.date).isAfter(
        getDateFromMonthsAgo(
          chartLookbackMonths,
          props.month ?? dayjs().toDate(),
        ),
        "month",
      ),
    )
    .filter((transaction) => {
      if (
        !props.category ||
        getIsParentCategory(props.category, allTransactionCategories)
      ) {
        return areStringsEqual(
          transaction.category ?? "",
          props.category ?? "",
        );
      }
      return areStringsEqual(
        transaction.subcategory ?? "",
        props.category ?? "",
      );
    });

  const transactionsForCategoryForCurrentMonth =
    transactionsForCategory?.filter((transaction) =>
      dayjs(transaction.date).isSame(props.month, "month"),
    );

  const forecastForCategory = (forecastQuery.data ?? []).filter(
    (occurrence) => {
      if (
        !props.category ||
        getIsParentCategory(props.category, allTransactionCategories)
      ) {
        return areStringsEqual(occurrence.category ?? "", props.category ?? "");
      }
      return areStringsEqual(occurrence.subcategory ?? "", props.category);
    },
  );

  const chartMonths = Array.from({ length: chartLookbackMonths }, (_, i) =>
    getDateFromMonthsAgo(i, props.month ?? dayjs().toDate()),
  );

  const isExpenseCategory = areStringsEqual(
    getCategoryType(props.category ?? ""),
    CategoryTypes.Expense,
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={
        <PrimaryHeading component="span" order={4}>
          {t("budget_details")}
        </PrimaryHeading>
      }
    >
      {transactionsQuery.isPending ||
      props.month === null ||
      props.category === null ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="1rem">
          <Group justify="space-between" align="center">
            <Stack gap={0}>
              <DimmedText size="xs">{t("category")}</DimmedText>
              <PrimaryText size="lg">
                {categoryIcon.length > 0 ? `${categoryIcon} ` : ""}
                {props.category ?? t("no_category")}
              </PrimaryText>
            </Stack>
            <Stack gap={0}>
              <DimmedText size="xs">{t("month")}</DimmedText>
              <PrimaryText size="lg">
                {dayjs(props.month).format("MMMM YYYY")}
              </PrimaryText>
            </Stack>
          </Group>
          <Accordion elevation={1}>
            <Accordion.Item
              title={
                <PrimaryHeading order={5} size="md">
                  {isExpenseCategory ? t("expense_trends") : t("income_trends")}
                </PrimaryHeading>
              }
            >
              <MonthlySpendingChart
                transactions={transactionsForCategory ?? []}
                months={chartMonths}
                includeYAxis={false}
                invertData={isExpenseCategory}
              />
            </Accordion.Item>
            <Accordion.Item
              title={
                <PrimaryHeading order={5} size="md">
                  {t("recent_transactions")}
                </PrimaryHeading>
              }
            >
              <TransactionCards
                transactions={transactionsForCategoryForCurrentMonth ?? []}
                categories={allTransactionCategories}
              />
            </Accordion.Item>
            <Accordion.Item
              title={
                <PrimaryHeading order={5} size="md">
                  {t("projected")}
                </PrimaryHeading>
              }
            >
              {forecastQuery.isPending ? (
                <Skeleton height={35} radius="md" />
              ) : forecastForCategory.length > 0 ? (
                <Stack gap="0.25rem">
                  {forecastForCategory.map((occurrence) => (
                    <Group
                      key={`${occurrence.ruleID}-${occurrence.date}`}
                      justify="space-between"
                    >
                      <PrimaryText size="sm">
                        {dayjs(occurrence.date).format("LL")}
                      </PrimaryText>
                      <StatusText size="sm" amount={occurrence.amount}>
                        <SensitiveAmount amount={occurrence.amount} />
                      </StatusText>
                    </Group>
                  ))}
                </Stack>
              ) : (
                <DimmedText size="sm">
                  {t("no_projected_transactions")}
                </DimmedText>
              )}
            </Accordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default BudgetDetails;
