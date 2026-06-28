import { Stack } from "@mantine/core";
import React from "react";
import { DatesRangeValue } from "@mantine/dates";
import { mantineDateFormat } from "~/helpers/datetime";
import AccountsSelectHeader from "~/components/AccountsSelectHeader/AccountsSelectHeader";
import NetWorthChart from "~/components/Charts/NetWorthChart/NetWorthChart";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useBalancesQuery } from "~/hooks/queries/useBalancesQuery";

const NetWorthTab = (): React.ReactNode => {
  const { dayjs } = useLocale();

  const [selectedAccountIds, setSelectedAccountIds] = React.useState<string[]>(
    [],
  );
  const [dateRange, setDateRange] = React.useState<DatesRangeValue<string>>([
    dayjs().subtract(1, "month").format(mantineDateFormat),
    dayjs().format(mantineDateFormat),
  ]);

  const balancesQuery = useBalancesQuery({
    accountIds: selectedAccountIds,
    enabled: selectedAccountIds.length > 0,
  });
  const accountsQuery = useAccountsQuery();

  return (
    <Stack p="0.5rem">
      <AccountsSelectHeader
        selectedAccountIds={selectedAccountIds}
        setSelectedAccountIds={setSelectedAccountIds}
        dateRange={dateRange}
        setDateRange={setDateRange}
      />
      <NetWorthChart
        accounts={accountsQuery.data ?? []}
        balances={balancesQuery.data ?? []}
        dateRange={dateRange}
        isPending={accountsQuery.isPending || balancesQuery.isPending}
      />
    </Stack>
  );
};

export default NetWorthTab;
