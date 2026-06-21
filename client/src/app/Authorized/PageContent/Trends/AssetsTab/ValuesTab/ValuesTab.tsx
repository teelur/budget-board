import { Stack } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import { useQueries } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import AssetsSelectHeader from "~/components/AssetsSelectHeader/AssetsSelectHeader";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { mantineDateFormat } from "~/helpers/datetime";
import { IValueResponse } from "~/models/value";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { valuesQueryKey } from "~/helpers/requests";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";

const ValuesTab = (): React.ReactNode => {
  const { dayjs } = useLocale();
  const { request } = useAuth();
  const assetsQuery = useAssetsQuery();

  const [selectedAssetIds, setSelectedAssetIds] = React.useState<string[]>([]);
  const [dateRange, setDateRange] = React.useState<DatesRangeValue<string>>([
    dayjs().subtract(1, "month").format(mantineDateFormat),
    dayjs().format(mantineDateFormat),
  ]);

  const valuesQuery = useQueries({
    queries: selectedAssetIds.map((assetId: string) => ({
      queryKey: [valuesQueryKey, assetId],
      queryFn: async (): Promise<IValueResponse[]> => {
        const res: AxiosResponse = await request({
          url: "/api/value",
          method: "GET",
          params: { assetId },
        });

        if (res.status === 200) {
          return res.data as IValueResponse[];
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
