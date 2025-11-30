import CategorySelect from "~/components/Select/CategorySelect/CategorySelect";
import classes from "./FilterCard.module.css";

import AccountSelectInput from "~/components/AccountSelectInput";
import { Flex, Stack, Text, Button, Card } from "@mantine/core";
import { DatePickerInput, DatesRangeValue } from "@mantine/dates";
import { Filters } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import dayjs from "dayjs";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";

interface FilterCardProps {
  categories: ICategory[];
}

const FilterCard = (props: FilterCardProps): React.ReactNode => {
  const { transactionFilters, setTransactionFilters, isFiltersPanelOpen } =
    useTransactionFilters();

  if (!isFiltersPanelOpen) {
    return null;
  }

  return (
    <Card
      p="0.5rem"
      radius="md"
      bg="var(--mantine-color-content-background)"
      withBorder
    >
      <Stack gap="0.5rem">
        <Text fw={600} size="md">
          Filters
        </Text>
        <Flex
          className={classes.group}
          direction={{ base: "column", sm: "row" }}
          wrap="nowrap"
          gap="md"
        >
          <DatePickerInput
            w={{ base: "100%", sm: "25%" }}
            type="range"
            placeholder="Pick a date range"
            value={transactionFilters.dateRange}
            onChange={(dateRange: DatesRangeValue<string>) => {
              const parsedDateRange: [Date | null, Date | null] = [
                dateRange[0] ? dayjs(dateRange[0]).toDate() : null,
                dateRange[1] ? dayjs(dateRange[1]).toDate() : null,
              ];
              const newFilters = new Filters();
              newFilters.accounts = transactionFilters.accounts;
              newFilters.category = transactionFilters.category;
              newFilters.dateRange = parsedDateRange;
              setTransactionFilters(newFilters);
            }}
            clearable
          />
          <AccountSelectInput
            w={{ base: "100%", sm: "50%" }}
            selectedAccountIds={transactionFilters.accounts}
            setSelectedAccountIds={(newAccountIds: string[]) => {
              const newFilters = new Filters();
              newFilters.accounts = newAccountIds;
              newFilters.category = transactionFilters.category;
              newFilters.dateRange = transactionFilters.dateRange;
              setTransactionFilters(newFilters);
            }}
            hideHidden
          />
          <CategorySelect
            w={{ base: "100%", sm: "20%" }}
            categories={props.categories}
            value={transactionFilters.category}
            onChange={(val) => {
              const newFilters = new Filters();
              newFilters.accounts = transactionFilters.accounts;
              newFilters.category = val;
              newFilters.dateRange = transactionFilters.dateRange;
              setTransactionFilters(newFilters);
            }}
            withinPortal
            includeUncategorized
          />
          <Button
            w={{ base: "100%", sm: "130px" }}
            variant={
              transactionFilters.isEqual(new Filters()) ? "outline" : "primary"
            }
            onClick={() => {
              setTransactionFilters(new Filters());
            }}
          >
            Clear Filters
          </Button>
        </Flex>
      </Stack>
    </Card>
  );
};

export default FilterCard;
