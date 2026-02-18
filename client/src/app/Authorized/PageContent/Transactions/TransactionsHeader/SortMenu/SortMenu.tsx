import classes from "./SortMenu.module.css";

import SortButton, { SortDirection } from "~/components/SortButton";
import { Flex, Group } from "@mantine/core";
import React from "react";
import { SortOption, Sorts } from "./SortMenuHelpers";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

interface SortMenuProps {
  currentSort: Sorts;
  setCurrentSort: (newCurrentSort: Sorts) => void;
  sortDirection: SortDirection;
  setSortDirection: (newSortDirection: SortDirection) => void;
}

const SortMenu = (props: SortMenuProps): React.ReactNode => {
  const { t } = useTranslation();

  const SortOptions: SortOption[] = [
    {
      value: Sorts.Date,
      label: t("date"),
    },
    {
      value: Sorts.Merchant,
      label: t("merchant_name"),
    },
    {
      value: Sorts.Category,
      label: t("category"),
    },
    {
      value: Sorts.Amount,
      label: t("amount"),
    },
  ];

  const ToggleSortDirection = (sortDirection: SortDirection): SortDirection => {
    switch (sortDirection) {
      case SortDirection.None:
        return SortDirection.Ascending;
      case SortDirection.Ascending:
        return SortDirection.Decending;
      case SortDirection.Decending:
        return SortDirection.Ascending;
    }
  };

  return (
    <Flex
      className={classes.container}
      gap="0.5rem"
      direction={{ base: "column", sm: "row" }}
      align={{ base: "start", sm: "center" }}
    >
      <DimmedText size="sm">{t("sort_by")}</DimmedText>
      <Group gap="0.5rem">
        {SortOptions.map((sortOption: SortOption) => (
          <SortButton
            key={sortOption.value}
            label={sortOption.label}
            variant="light"
            sortDirection={
              sortOption.value === props.currentSort
                ? props.sortDirection
                : SortDirection.None
            }
            onClick={() => {
              if (sortOption.value !== props.currentSort) {
                // When selecting a new sort option, should always go to decending.
                props.setSortDirection(SortDirection.Decending);
              } else {
                props.setSortDirection(
                  ToggleSortDirection(props.sortDirection),
                );
              }
              props.setCurrentSort(sortOption.value);
            }}
          />
        ))}
      </Group>
    </Flex>
  );
};

export default SortMenu;
