import { Accordion, Drawer, Group, Skeleton, Stack, Text } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import MonthlySpendingChart from "~/components/Charts/MonthlySpendingChart/MonthlySpendingChart";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getDateFromMonthsAgo } from "~/helpers/datetime";
import { areStringsEqual } from "~/helpers/utils";
import { ICategoryResponse } from "~/models/category";
import {
  defaultTransactionCategories,
  ITransaction,
} from "~/models/transaction";
import TransactionCards from "./TransactionCards/TransactionCards";
import dayjs from "dayjs";

interface BudgetDetailsProps {
  isOpen: boolean;
  close: () => void;
  category: string | null;
  month: Date | null;
}

const BudgetDetails = (props: BudgetDetailsProps): React.ReactNode => {
  const chartLookbackMonths = 6;

  const { request } = React.useContext<any>(AuthContext);
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

  const transactionCategoriesQuery = useQuery({
    queryKey: ["transactionCategories"],
    queryFn: async () => {
      const res = await request({
        url: "/api/transactionCategory",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ICategoryResponse[];
      }

      return undefined;
    },
  });

  const transactionCategoriesWithCustom = defaultTransactionCategories.concat(
    transactionCategoriesQuery.data ?? []
  );

  const transactionsForCategory = transactionsQuery.data
    ?.filter((transaction) =>
      dayjs(transaction.date).isAfter(
        getDateFromMonthsAgo(chartLookbackMonths, props.month ?? new Date()),
        "month"
      )
    )
    .filter((transaction) => {
      if (
        getIsParentCategory(
          props.category ?? "",
          transactionCategoriesWithCustom
        )
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
    getParentCategory(props.category ?? "", transactionCategoriesWithCustom),
    "income"
  );

  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={
        <Text size="lg" fw={600}>
          Budget Details
        </Text>
      }
    >
      {transactionsQuery.isPending || transactionCategoriesQuery.isPending ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Stack gap="1rem">
          <Group justify="space-between" align="center">
            <Stack gap={0}>
              <Text size="xs" fw={500} c="dimmed">
                Category
              </Text>
              <Text size="lg" fw={600}>
                {props.category ?? "No Category"}
              </Text>
            </Stack>
            <Stack gap={0}>
              <Text size="xs" fw={500} c="dimmed">
                Month
              </Text>
              <Text size="lg" fw={600}>
                {props.month?.toLocaleString("default", {
                  month: "long",
                  year: "numeric",
                })}
              </Text>
            </Stack>
          </Group>
          <Accordion
            variant="separated"
            defaultValue={["chart", "transactions"]}
            multiple
          >
            <Accordion.Item value="chart">
              <Accordion.Control>
                <Text>{isExpenseCategory ? "Expense" : "Income"} Trends</Text>
              </Accordion.Control>
              <Accordion.Panel>
                <MonthlySpendingChart
                  transactions={transactionsForCategory ?? []}
                  months={chartMonths}
                  includeYAxis={false}
                  invertData={isExpenseCategory}
                />
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item value="transactions">
              <Accordion.Control>Recent Transactions</Accordion.Control>
              <Accordion.Panel>
                <TransactionCards
                  transactions={transactionsForCategoryForCurrentMonth ?? []}
                  categories={transactionCategoriesWithCustom}
                />
              </Accordion.Panel>
            </Accordion.Item>
          </Accordion>
        </Stack>
      )}
    </Drawer>
  );
};

export default BudgetDetails;
