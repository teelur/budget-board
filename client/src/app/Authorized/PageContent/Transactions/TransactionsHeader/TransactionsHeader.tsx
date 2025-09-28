import classes from "./TransactionsHeader.module.css";

import { ActionIcon, Button, Flex, Group, Stack } from "@mantine/core";
import { FilterIcon, SettingsIcon } from "lucide-react";
import React from "react";
import SortMenu from "./SortMenu/SortMenu";
import { SortDirection } from "~/components/SortButton";
import { Filters } from "~/models/transaction";
import { Sorts } from "./SortMenu/SortMenuHelpers";
import FilterCard from "./FilterCard/FilterCard";
import { useDisclosure } from "@mantine/hooks";
import TransactionsSettings from "./TransactionsSettings/TransactionsSettings";
import { ICategory } from "~/models/category";
import CreateTransactionModal from "./CreateTransactionModal/CreateTransactionModal";
import ImportTransactionsModal from "./ImportTransactionsModal/ImportTransactionsModal";

interface TransactionsHeaderProps {
  sort: Sorts;
  setSort: (newSort: Sorts) => void;
  sortDirection: SortDirection;
  setSortDirection: (newSortDirection: SortDirection) => void;
  filters: Filters;
  setFilters: (newFilters: Filters) => void;
  categories: ICategory[];
}

const TransactionsHeader = (
  props: TransactionsHeaderProps
): React.ReactNode => {
  const [settingsOpen, { open, close }] = useDisclosure(false);
  const [isFilterCardOpen, { toggle }] = useDisclosure();

  return (
    <Stack className={classes.root}>
      <Flex className={classes.header}>
        <SortMenu
          currentSort={props.sort}
          setCurrentSort={props.setSort}
          sortDirection={props.sortDirection}
          setSortDirection={props.setSortDirection}
        />
        <Group className={classes.buttonGroup}>
          <ImportTransactionsModal />
          <Button
            variant={isFilterCardOpen ? "outline" : "primary"}
            size="sm"
            rightSection={<FilterIcon size="1rem" />}
            onClick={toggle}
          >
            Filter
          </Button>
          <CreateTransactionModal />
          <ActionIcon variant="subtle" size="input-sm" onClick={open}>
            <SettingsIcon />
          </ActionIcon>
          <TransactionsSettings modalOpened={settingsOpen} closeModal={close} />
        </Group>
      </Flex>
      <FilterCard
        isOpen={isFilterCardOpen}
        categories={props.categories}
        filters={props.filters}
        setFilters={props.setFilters}
      />
    </Stack>
  );
};

export default TransactionsHeader;
