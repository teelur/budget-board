import { Stack } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import React from "react";
import AssetsSelectHeader from "~/components/AssetsSelectHeader/AssetsSelectHeader";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { mantineDateFormat } from "~/helpers/datetime";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";
import { useValuesQuery } from "~/hooks/queries/useValuesQuery";

const ValuesTab = (): React.ReactNode => {
  const { dayjs } = useLocale();
  const assetsQuery = useAssetsQuery();

  const [selectedAssetIds, setSelectedAssetIds] = React.useState<string[]>([]);

  const valuesQuery = useValuesQuery({
    assetIds: selectedAssetIds,
  });

  const [dateRange, setDateRange] = React.useState<DatesRangeValue<string>>([
    dayjs().subtract(1, "month").format(mantineDateFormat),
    dayjs().format(mantineDateFormat),
  ]);

  return (
    <Stack p="0.5rem" gap="1rem">
      <AssetsSelectHeader
        selectedAssetIds={selectedAssetIds}
        setSelectedAssetIds={setSelectedAssetIds}
        dateRange={dateRange}
        setDateRange={setDateRange}
        filters={["Checking", "Savings", "Investment", "Cash"]}
      />
      <ValueChart
        values={(valuesQuery.data ?? []).map((value) => ({
          ...value,
          parentId: value.assetID || "",
        }))}
        items={(assetsQuery.data ?? [])
          .filter((a) => selectedAssetIds.includes(a.id))
          .map((asset) => ({
            id: asset.id,
            name: asset.name,
          }))}
        dateRange={dateRange}
        isPending={valuesQuery.isPending || assetsQuery.isPending}
      />
    </Stack>
  );
};

export default ValuesTab;
