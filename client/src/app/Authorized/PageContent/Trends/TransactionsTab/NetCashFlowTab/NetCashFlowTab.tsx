import React from "react";
import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { buildTimeToMonthlyTotalsMap } from "~/helpers/transactions";
import NetCashFlowChart from "~/components/Charts/NetCashFlowChart/NetCashFlowChart";
import { Stack } from "@mantine/core";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import SelectLastNMonths from "~/components/SelectLastNMonths/SelectLastNMonths";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";

const NetCashFlowTab = (): React.ReactNode => {
  const monthButtons = [3, 6, 12];

  const { dayjs } = useLocale();

  const [selectedMonths, setSelectedMonths] = React.useState<Date[]>([
    dayjs().subtract(1, "month").startOf("month").toDate(),
    dayjs().startOf("month").toDate(),
  ]);

  const transactionsQuery = useTransactionsQuery({
    selectedDates: selectedMonths.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });
  ``;

  return (
    <Stack p={"0.5rem"}>
      <MonthToolcards
        selectedDates={selectedMonths}
        setSelectedDates={setSelectedMonths}
        timeToMonthlyTotalsMap={buildTimeToMonthlyTotalsMap(
          selectedMonths,
          transactionsQuery.data ?? [],
        )}
        isPending={transactionsQuery.isPending}
        allowSelectMultiple
      />
      <SelectLastNMonths
        monthButtons={monthButtons}
        setSelectedMonths={setSelectedMonths}
      />
      <NetCashFlowChart
        transactions={transactionsQuery.data ?? []}
        months={selectedMonths}
        isPending={transactionsQuery.isPending}
      />
    </Stack>
  );
};

export default NetCashFlowTab;
