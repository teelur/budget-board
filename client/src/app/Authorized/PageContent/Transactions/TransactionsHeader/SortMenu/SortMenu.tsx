import classes from "./SortMenu.module.css";

import { Flex, Group, Menu } from "@mantine/core";
import { BodyText, Button } from "@teelur/budget-board-ui";
import {
  ArrowDownIcon,
  ArrowUpDownIcon,
  ArrowUpIcon,
  CheckIcon,
} from "lucide-react";
import SortButton, { SortDirection } from "~/components/SortButton";
import React from "react";
import { SortOption, Sorts } from "./SortMenuHelpers";
import { useTranslation } from "react-i18next";

interface SortMenuProps {
  currentSort: Sorts;
  setCurrentSort: (newCurrentSort: Sorts) => void;
  sortDirection: SortDirection;
  setSortDirection: (newSortDirection: SortDirection) => void;
  compact?: boolean;
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

  const handleSortClick = (sortOption: SortOption) => {
    if (sortOption.value !== props.currentSort) {
      props.setSortDirection(SortDirection.Decending);
    } else {
      props.setSortDirection(ToggleSortDirection(props.sortDirection));
    }

    props.setCurrentSort(sortOption.value);
  };

  if (props.compact) {
    const currentSortLabel =
      SortOptions.find((sortOption) => sortOption.value === props.currentSort)
        ?.label ?? t("sort_by");

    return (
      <Menu withinPortal>
        <Menu.Target>
          <Button
            variant="filled"
            color="secondary"
            size="compact-sm"
            aria-label={`${t("sort_by")}: ${currentSortLabel}`}
          >
            <ArrowUpDownIcon size="1rem" />
          </Button>
        </Menu.Target>
        <Menu.Dropdown>
          <Menu.Label>{t("sort_by")}</Menu.Label>
          {SortOptions.map((sortOption) => {
            const isSelected = sortOption.value === props.currentSort;

            return (
              <Menu.Item
                key={sortOption.value}
                leftSection={isSelected ? <CheckIcon size="1rem" /> : null}
                rightSection={
                  !isSelected
                    ? null
                    : props.sortDirection === SortDirection.Ascending
                      ? <ArrowUpIcon size="1rem" />
                      : props.sortDirection === SortDirection.Decending
                        ? <ArrowDownIcon size="1rem" />
                        : <ArrowUpDownIcon size="1rem" />
                }
                onClick={() => handleSortClick(sortOption)}
              >
                {sortOption.label}
              </Menu.Item>
            );
          })}
        </Menu.Dropdown>
      </Menu>
    );
  }

  return (
    <Flex
      className={classes.container}
      gap="0.5rem"
      direction={{ base: "column", sm: "row" }}
      align={{ base: "start", sm: "center" }}
    >
      <BodyText tone="secondary" size="sm" fw={500}>
        {t("sort_by")}
      </BodyText>
      <Group gap="0.5rem">
        {SortOptions.map((sortOption: SortOption) => (
          <SortButton
            variant="filled"
            color="secondary"
            size="compact-sm"
            key={sortOption.value}
            label={sortOption.label}
            sortDirection={
              sortOption.value === props.currentSort
                ? props.sortDirection
                : SortDirection.None
            }
            onClick={() => handleSortClick(sortOption)}
          />
        ))}
      </Group>
    </Flex>
  );
};

export default SortMenu;
