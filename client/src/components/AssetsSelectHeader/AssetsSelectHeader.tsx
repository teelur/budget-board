import { Button, Group } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import React from "react";
import DatePickerInput from "../core/Input/DatePickerInput/DatePickerInput";
import AssetSelect from "../core/Select/AssetSelect/AssetSelect";
import { useTranslation } from "react-i18next";
import SelectLastNMonthsRange from "../SelectLastNMonthsRange/SelectLastNMonthsRange";
import { useAssetsQuery } from "~/hooks/queries/useAssetsQuery";

interface AssetsSelectHeaderProps {
  selectedAssetIds: string[];
  setSelectedAssetIds: (assetIds: string[]) => void;
  dateRange: DatesRangeValue<string>;
  setDateRange: React.Dispatch<React.SetStateAction<DatesRangeValue<string>>>;
  filters?: string[];
}

const AssetsSelectHeader = (
  props: AssetsSelectHeaderProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const assetsQuery = useAssetsQuery();

  return (
    <Group>
      <Group>
        <DatePickerInput
          type="range"
          value={props.dateRange}
          onChange={props.setDateRange}
          miw={200}
          elevation={1}
        />
        <AssetSelect
          selectedAssetIds={props.selectedAssetIds}
          setSelectedAssetIds={props.setSelectedAssetIds}
          hideHidden
          flex={1}
          miw="230px"
          elevation={1}
        />
        <Button
          onClick={() => {
            props.setSelectedAssetIds(
              assetsQuery.data
                ?.filter((asset) => !asset?.hide)
                ?.map((asset) => asset.id) ?? [],
            );
          }}
        >
          {t("select_all")}
        </Button>
        <Button onClick={() => props.setSelectedAssetIds([])}>
          {t("clear_all")}
        </Button>
      </Group>
      <SelectLastNMonthsRange
        monthButtons={[3, 6, 12]}
        setDateRange={props.setDateRange}
      />
    </Group>
  );
};

export default AssetsSelectHeader;
