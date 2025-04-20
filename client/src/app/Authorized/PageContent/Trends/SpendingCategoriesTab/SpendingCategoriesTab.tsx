import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { getUniqueYears, initCurrentMonth } from "~/helpers/datetime";
import {
  buildTimeToMonthlyTotalsMap,
  filterHiddenTransactions,
} from "~/helpers/transactions";
import { Button, Flex, Group, Stack } from "@mantine/core";
import {
  defaultTransactionCategories,
  ITransaction,
} from "~/models/transaction";
import { useQueries, useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import SpendingCategoriesChart from "~/components/Charts/SpendingCategoriesChart/SpendingCategoriesChart";
import { ICategoryResponse } from "~/models/category";

const SpendingCategoriesTab = (): React.ReactNode => {
  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    initCurrentMonth(),
  ]);

  // Querying by year is the best balance of covering probable dates a user will select,
  // while also not potentially querying for a large amount of data.
  const { request } = React.useContext<any>(AuthContext);
  const transactionsQuery = useQueries({
    queries: getUniqueYears(selectedMonths).map((year: number) => ({
      queryKey: ["transactions", { year }],
      queryFn: async (): Promise<ITransaction[]> => {
        const res: AxiosResponse = await request({
          url: "/api/transaction",
          method: "GET",
          params: { year },
        });

        if (res.status === 200) {
          return res.data as ITransaction[];
        }

        return [];
      },
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data ?? []).flat(1),
        isPending: results.some((result) => result.isPending),
      };
    },
  });

  // We need to filter out the transactions labelled with 'Hide From Budgets'
  const transactionsWithoutHidden = filterHiddenTransactions(
    transactionsQuery.data ?? []
  );

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

  return (
    <Stack p="0.5rem" gap="1rem">
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        timeToMonthlyTotalsMap={buildTimeToMonthlyTotalsMap(
          selectedMonths,
          transactionsWithoutHidden
        )}
        isPending={transactionsQuery.isPending}
        allowSelectMultiple
      />
      <Group w="100%" justify="end">
        <Button
          size="compact-sm"
          variant="primary"
          onClick={() => setSelectedMonths([])}
        >
          Clear Selection
        </Button>
      </Group>
      <Flex justify="center">
        <SpendingCategoriesChart
          transactions={transactionsWithoutHidden}
          categories={transactionCategoriesWithCustom}
        />
      </Flex>
    </Stack>
  );
};

export default SpendingCategoriesTab;
