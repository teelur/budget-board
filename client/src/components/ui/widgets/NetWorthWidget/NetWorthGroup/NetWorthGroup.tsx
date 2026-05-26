import { Stack } from "@mantine/core";
import {
  INetWorthWidgetGroup,
  INetWorthWidgetLine,
} from "~/models/widgetSettings";
import NetWorthItem from "./NetWorthItem/NetWorthItem";
import { calculateLineTotal } from "~/helpers/widgets";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { IAssetResponse } from "~/models/asset";
import { IAccountResponse } from "~/models/account";

interface NetWorthGroupProps {
  netWorthWidgetGroup: INetWorthWidgetGroup;
  validAccounts: IAccountResponse[];
  validAssets: IAssetResponse[];
  orderedGroups: INetWorthWidgetGroup[];
}

const NetWorthGroup = ({
  netWorthWidgetGroup,
  validAccounts,
  validAssets,
  orderedGroups,
}: NetWorthGroupProps): React.ReactNode => {
  const { allAccountTypes } = useAccountTypes();
  const { allAssetTypes } = useAssetTypes();
  const { preferredCurrency } = useUserSettings();

  const sortedLines = netWorthWidgetGroup.lines
    .slice()
    .sort(
      (a: INetWorthWidgetLine, b: INetWorthWidgetLine) => a.index - b.index,
    );

  return (
    <Stack px="0.5rem" gap={0} justify="center">
      {sortedLines.map((line: INetWorthWidgetLine) => (
        <NetWorthItem
          key={line.id}
          title={line.name}
          totalBalance={calculateLineTotal(
            line,
            validAccounts,
            validAssets,
            orderedGroups.flatMap((g) => g.lines),
            allAccountTypes,
            allAssetTypes,
          )}
          userCurrency={preferredCurrency ?? "USD"}
        />
      ))}
    </Stack>
  );
};

export default NetWorthGroup;
