import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { Button, Group } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { IAssetResponse } from "~/models/asset";
import DatePickerInput from "../core/Input/DatePickerInput/DatePickerInput";
import AssetSelect from "../core/Select/AssetSelect/AssetSelect";
import { useTranslation } from "react-i18next";

interface AssetsSelectHeaderProps {
  selectedAssetIds: string[];
  setSelectedAssetIds: (assetIds: string[]) => void;
  dateRange: DatesRangeValue<string>;
  setDateRange: (dateRange: DatesRangeValue<string>) => void;
  filters?: string[];
}

const AssetsSelectHeader = (
  props: AssetsSelectHeaderProps
): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

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
    <Group>
      <DatePickerInput
        type="range"
        value={props.dateRange}
        onChange={props.setDateRange}
        elevation={1}
      />
      <AssetSelect
        selectedAssetIds={props.selectedAssetIds}
        setSelectedAssetIds={props.setSelectedAssetIds}
        hideHidden
        miw="230px"
        maw="400px"
        elevation={1}
      />
      <Button
        onClick={() => {
          props.setSelectedAssetIds(
            assetsQuery.data
              ?.filter((asset: IAssetResponse) => !asset?.hide)
              ?.map((asset) => asset.id) ?? []
          );
        }}
      >
        {t("select_all")}
      </Button>
      <Button onClick={() => props.setSelectedAssetIds([])}>
        {t("clear_all")}
      </Button>
    </Group>
  );
};

export default AssetsSelectHeader;
