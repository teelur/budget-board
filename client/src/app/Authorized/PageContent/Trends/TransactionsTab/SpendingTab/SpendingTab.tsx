import classes from "./SpendingTab.module.css";

import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import SpendingChart from "~/components/Charts/SpendingChart/SpendingChart";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import {
  getDateFromMonthsAgo,
  getUniqueYears,
  initCurrentMonth,
} from "~/helpers/datetime";
import {
  buildTimeToMonthlyTotalsMap,
  filterHiddenTransactions,
} from "~/helpers/transactions";
import { Button, Group, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";

const SpendingTab = (): React.ReactNode => {
  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    getDateFromMonthsAgo(1),
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

  return (
    <Stack className={classes.root}>
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
      <SpendingChart
        transactions={transactionsWithoutHidden}
        months={selectedMonths}
        isPending={transactionsQuery.isPending}
        includeGrid
        includeYAxis
      />
    </Stack>
  );
};

export default SpendingTab;
