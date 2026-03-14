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
import { Stack } from "@mantine/core";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";

const NetCashFlowTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

  const { dayjs } = useLocale();

  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().subtract(1, "month").startOf("month").toDate(),
    dayjs().startOf("month").toDate(),
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
    <Stack p={"0.5rem"}>
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
      <SelectLastNMonths
        monthButtons={monthButtons}
        setSelectedMonths={setSelectedMonths}
      />
      <NetCashFlowChart
        transactions={transactionsWithoutHidden}
        months={selectedMonths}
        isPending={transactionsQuery.isPending}
      />
    </Stack>
  );
};

export default NetCashFlowTab;
