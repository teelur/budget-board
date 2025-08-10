import classes from "./SortMenu.module.css";

import SortButton, { SortDirection } from "~/components/SortButton";
import { Flex, Group, Text } from "@mantine/core";
import React from "react";
import { SortOption, SortOptions, Sorts } from "./SortMenuHelpers";

interface SortMenuProps {
  currentSort: Sorts;
  setCurrentSort: (newCurrentSort: Sorts) => void;
  sortDirection: SortDirection;
  setSortDirection: (newSortDirection: SortDirection) => void;
}

const SortMenu = (props: SortMenuProps): React.ReactNode => {
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
      direction={{ base: "column", sm: "row" }}
      align={{ base: "start", sm: "center" }}
    >
      <Text c="dimmed" fw={600} size="sm">
        Sort By
      </Text>
      <Group className={classes.sortButtons}>
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
                  ToggleSortDirection(props.sortDirection)
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
