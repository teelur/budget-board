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
import CreateTransactionModal from "./CreateTransactionModal/CreateTransactionModal";
import ImportTransactionsModal from "./ImportTransactionsModal/ImportTransactionsModal";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { useTranslation } from "react-i18next";
import ExportTransactionsModal from "./ExportTransactionsModal/ExportTransactionsModal";
import { useNavigate } from "react-router";

interface TransactionsHeaderProps {
  sort: Sorts;
  setSort: (newSort: Sorts) => void;
  sortDirection: SortDirection;
  setSortDirection: (newSortDirection: SortDirection) => void;
}

const TransactionsHeader = (
  props: TransactionsHeaderProps,
): React.ReactNode => {
  const navigate = useNavigate();

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
          <ExportTransactionsModal />
          <Button
            variant={isFiltersPanelOpen ? "outline" : "primary"}
            size="xs"
            rightSection={<FilterIcon size="1rem" />}
            onClick={toggleFiltersPanel}
          >
            {t("filters")}
          </Button>
          <CreateTransactionModal />
          <ActionIcon
            variant="subtle"
            size="input-xs"
            onClick={() => navigate("/transactions/settings")}
          >
            <SettingsIcon />
          </ActionIcon>
        </Group>
      </Flex>
      <Collapse expanded={isFiltersPanelOpen} transitionDuration={100}>
        <FilterCard />
      </Collapse>
    </Stack>
  );
};

export default TransactionsHeader;
