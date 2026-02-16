import { Stack } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import { useQueries, useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import AssetsSelectHeader from "~/components/AssetsSelectHeader/AssetsSelectHeader";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import ValueChart from "~/components/Charts/ValueChart/ValueChart";
import { mantineDateFormat } from "~/helpers/datetime";
import { IAssetResponse } from "~/models/asset";
import { IValueResponse } from "~/models/value";
import { useDate } from "~/providers/DateProvider/DateProvider";

const ValuesTab = (): React.ReactNode => {
  const { dayjs } = useDate();
  const { request } = useAuth();

  const [selectedAssetIds, setSelectedAssetIds] = React.useState<string[]>([]);
  const [dateRange, setDateRange] = React.useState<DatesRangeValue<string>>([
    dayjs().subtract(1, "month").startOf("month").format(mantineDateFormat),
    dayjs().startOf("month").format(mantineDateFormat),
  ]);

  const valuesQuery = useQueries({
    queries: selectedAssetIds.map((assetId: string) => ({
      queryKey: ["values", assetId],
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

  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async (): Promise<IAssetResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAssetResponse[];
      }

      return [];
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
