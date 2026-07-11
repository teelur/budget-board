import { Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import MonthlySpendingChart from "~/components/Charts/MonthlySpendingChart/MonthlySpendingChart";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
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
  const { allTransactionCategories: transactionCategories } =
    useTransactionCategories();
  const transactionsQuery = useTransactionsQuery();

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
        getIsParentCategory(props.category, transactionCategories)
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

  const chartMonths = Array.from({ length: chartLookbackMonths }, (_, i) =>
    getDateFromMonthsAgo(i, props.month ?? dayjs().toDate()),
  );

  const isExpenseCategory = !areStringsEqual(
    getParentCategory(props.category ?? "", transactionCategories),
    "income",
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
                categories={transactionCategories}
              />
            </Accordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default BudgetDetails;
