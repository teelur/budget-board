import {
  Accordion as MantineAccordion,
  Group,
  Skeleton,
  Stack,
} from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import MonthlySpendingChart from "~/components/Charts/MonthlySpendingChart/MonthlySpendingChart";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { areStringsEqual } from "~/helpers/utils";
import { ITransaction } from "~/models/transaction";
import TransactionCards from "./TransactionCards/TransactionCards";
import dayjs from "dayjs";
import { filterHiddenTransactions } from "~/helpers/transactions";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Drawer from "~/components/core/Drawer/Drawer";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";

interface BudgetDetailsProps {
  isOpen: boolean;
  close: () => void;
  category: string | null;
  month: Date | null;
}

const BudgetDetails = (props: BudgetDetailsProps): React.ReactNode => {
  const chartLookbackMonths = 6;

  const { transactionCategories } = useTransactionCategories();
  const { request } = useAuth();

  const transactionsQuery = useQuery({
    queryKey: ["transactions", { getHidden: false }],
    queryFn: async (): Promise<ITransaction[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transaction",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ITransaction[];
      }

      return [];
    },
  });

  const transactionsForCategory = filterHiddenTransactions(
    transactionsQuery.data ?? []
  )
    .filter((transaction) =>
      dayjs(transaction.date).isAfter(
        getDateFromMonthsAgo(chartLookbackMonths, props.month ?? new Date()),
        "month"
      )
    )
    .filter((transaction) => {
      if (
        !props.category ||
        getIsParentCategory(props.category, transactionCategories)
      ) {
        return areStringsEqual(
          transaction.category ?? "",
          props.category ?? ""
        );
      }
      return areStringsEqual(
        transaction.subcategory ?? "",
        props.category ?? ""
      );
    });

  const transactionsForCategoryForCurrentMonth =
    transactionsForCategory?.filter((transaction) =>
      dayjs(transaction.date).isSame(props.month, "month")
    );

  const chartMonths = Array.from({ length: chartLookbackMonths }, (_, i) =>
    getDateFromMonthsAgo(i, props.month ?? new Date())
  );

  const isExpenseCategory = !areStringsEqual(
    getParentCategory(props.category ?? "", transactionCategories),
    "income"
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={<PrimaryText size="lg">Budget Details</PrimaryText>}
    >
      {transactionsQuery.isPending ||
      props.month === null ||
      props.category === null ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="1rem">
          <Group justify="space-between" align="center">
            <Stack gap={0}>
              <DimmedText size="xs">Category</DimmedText>
              <PrimaryText size="lg">
                {props.category ?? "No Category"}
              </PrimaryText>
            </Stack>
            <Stack gap={0}>
              <DimmedText size="xs">Month</DimmedText>
              <PrimaryText size="lg">
                {props.month?.toLocaleString("default", {
                  month: "long",
                  year: "numeric",
                })}
              </PrimaryText>
            </Stack>
          </Group>
          <Accordion defaultValue={["chart", "transactions"]} elevation={1}>
            <MantineAccordion.Item value="chart">
              <MantineAccordion.Control>
                <PrimaryText size="md">
                  {isExpenseCategory ? "Expense" : "Income"} Trends
                </PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <MonthlySpendingChart
                  transactions={transactionsForCategory ?? []}
                  months={chartMonths}
                  includeYAxis={false}
                  invertData={isExpenseCategory}
                />
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
            <MantineAccordion.Item value="transactions">
              <MantineAccordion.Control>
                <PrimaryText size="md">Recent Transactions</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <TransactionCards
                  transactions={transactionsForCategoryForCurrentMonth ?? []}
                  categories={transactionCategories}
                />
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default BudgetDetails;
