import classes from "./NetCashFlowTab.module.css";

import React from "react";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { getUniqueYears } from "~/helpers/datetime";
import {
  buildTimeToMonthlyTotalsMap,
  filterHiddenTransactions,
} from "~/helpers/transactions";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQueries } from "@tanstack/react-query";
import { ITransaction } from "~/models/transaction";
import { AxiosResponse } from "axios";
import NetCashFlowChart from "~/components/Charts/NetCashFlowChart/NetCashFlowChart";
import { Button, Group, Stack } from "@mantine/core";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

const NetCashFlowTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

  const { t } = useTranslation();
  const { dayjs } = useDate();

  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().subtract(1, "month").toDate(),
    dayjs().toDate(),
  ]);

  // Querying by year is the best balance of covering probable dates a user will select,
  // while also not potentially querying for a large amount of data.
  const { request } = useAuth();
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
    transactionsQuery.data ?? [],
  );

  return (
    <Stack className={classes.root}>
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        timeToMonthlyTotalsMap={buildTimeToMonthlyTotalsMap(
          selectedMonths,
          transactionsWithoutHidden,
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
                  newMonths.push(dayjs().subtract(i, "month").toDate());
                  return newMonths;
                });
              }
            }}
          >
            {t("last_n_months", { count: months })}
          </Button>
        ))}
        <Button
          size="compact-sm"
          variant="primary"
          onClick={() => setSelectedMonths([])}
        >
          {t("clear_selection")}
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
