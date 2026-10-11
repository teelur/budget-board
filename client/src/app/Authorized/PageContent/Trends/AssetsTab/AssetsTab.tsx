import { Stack } from "@mantine/core";
import React from "react";
import { mantineDateFormat } from "~/helpers/datetime";
import AccountsSelectHeader from "~/components/AccountsSelectHeader/AccountsSelectHeader";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { AccountTypeClassification } from "~/models/account";
import { DatesRangeValue } from "@mantine/dates";
import { IItem } from "~/components/Charts/ValueChart/helpers/valueChart";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";
import { useBalancesQuery } from "~/hooks/queries/useBalancesQuery";

const AssetsTab = (): React.ReactNode => {
  const { dayjs } = useLocale();
  const { allAccountTypes } = useAccountTypes();

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

  const assetAccountTypes = allAccountTypes
    .filter((type) => type.classification === AccountTypeClassification.Asset)
    .map((type) => type.value);

  return (
    <Stack p="0.5rem">
      <AccountsSelectHeader
        selectedAccountIds={selectedAccountIds}
        setSelectedAccountIds={setSelectedAccountIds}
        dateRange={dateRange}
        setDateRange={setDateRange}
        filters={assetAccountTypes}
      />
      <ValueChart
        values={(balancesQuery.data ?? []).map((balance) => ({
          ...balance,
          parentId: balance.accountID || "",
        }))}
        items={(accountsQuery.data ?? [])
          .filter((a) => selectedAccountIds.includes(a.id))
          .map(
            (account) =>
              ({
                id: account.id,
                name: account.name,
              }) as IItem,
          )}
        dateRange={dateRange}
        isPending={balancesQuery.isPending || accountsQuery.isPending}
      />
    </Stack>
  );
};

export default AssetsTab;
