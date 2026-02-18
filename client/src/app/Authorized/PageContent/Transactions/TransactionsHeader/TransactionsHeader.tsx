import classes from "./TransactionsHeader.module.css";

import {
  ActionIcon,
  Button,
  Collapse,
  Flex,
  Group,
  Stack,
} from "@mantine/core";
import { FilterIcon, SettingsIcon } from "lucide-react";
import React from "react";
import SortMenu from "./SortMenu/SortMenu";
import { SortDirection } from "~/components/SortButton";
import { Sorts } from "./SortMenu/SortMenuHelpers";
import FilterCard from "./FilterCard/FilterCard";
import { useDisclosure } from "@mantine/hooks";
import TransactionsSettings from "./TransactionsSettings/TransactionsSettings";
import { ICategory } from "~/models/category";
import CreateTransactionModal from "./CreateTransactionModal/CreateTransactionModal";
import ImportTransactionsModal from "./ImportTransactionsModal/ImportTransactionsModal";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { useTranslation } from "react-i18next";

interface TransactionsHeaderProps {
  sort: Sorts;
  setSort: (newSort: Sorts) => void;
  sortDirection: SortDirection;
  setSortDirection: (newSortDirection: SortDirection) => void;
  categories: ICategory[];
}

const TransactionsHeader = (
  props: TransactionsHeaderProps,
): React.ReactNode => {
  const [settingsOpen, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();
  const { isFiltersPanelOpen, toggleFiltersPanel } = useTransactionFilters();

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
            variant={isFiltersPanelOpen ? "outline" : "primary"}
            size="sm"
            rightSection={<FilterIcon size="1rem" />}
            onClick={toggleFiltersPanel}
          >
            {t("filters")}
          </Button>
          <CreateTransactionModal />
          <ActionIcon variant="subtle" size="input-sm" onClick={open}>
            <SettingsIcon />
          </ActionIcon>
          <TransactionsSettings modalOpened={settingsOpen} closeModal={close} />
        </Group>
      </Flex>
      <Collapse in={isFiltersPanelOpen} transitionDuration={100}>
        <FilterCard categories={props.categories} />
      </Collapse>
    </Stack>
  );
};

export default TransactionsHeader;
