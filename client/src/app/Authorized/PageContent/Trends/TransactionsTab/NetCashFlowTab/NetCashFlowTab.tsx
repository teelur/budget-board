import classes from "./NetCashFlowTab.module.css";

import React from "react";
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
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQueries } from "@tanstack/react-query";
import { ITransaction } from "~/models/transaction";
import { AxiosResponse } from "axios";
import NetCashFlowChart from "~/components/Charts/NetCashFlowChart/NetCashFlowChart";
import { Button, Group, Stack } from "@mantine/core";

const NetCashFlowTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

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
        {monthButtons.map((months) => (
          <Button
            size="compact-sm"
            variant="light"
            key={months}
            onClick={() => {
              // Clear prior to adding new months to prevent duplicates.
              setSelectedMonths([]);
              for (let i = 0; i < months; i++) {
                setSelectedMonths((prev) => {
                  const newMonths = [...prev];
                  newMonths.push(getDateFromMonthsAgo(i));
                  return newMonths;
                });
              }
            }}
          >
            Last {months} Months
          </Button>
        ))}
        <Button
          size="compact-sm"
          variant="primary"
          onClick={() => setSelectedMonths([])}
        >
          Clear Selection
        </Button>
      </Group>
      <NetCashFlowChart
        transactions={transactionsWithoutHidden}
        months={selectedMonths}
        isPending={transactionsQuery.isPending}
      />
    </Stack>
  );
};

export default NetCashFlowTab;
