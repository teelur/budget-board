import CategorySelect from "~/components/CategorySelect";
import classes from "./FilterCard.module.css";

import AccountSelectInput from "~/components/AccountSelectInput";
import { Flex, Stack, Text, Button } from "@mantine/core";
import { DatePickerInput, DatesRangeValue } from "@mantine/dates";
import { Filters } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import dayjs from "dayjs";

interface FilterCardProps {
  isOpen: boolean;
  categories: ICategory[];
  filters: Filters;
  setFilters: (newFilters: Filters) => void;
}

const FilterCard = (props: FilterCardProps): React.ReactNode => {
  if (!props.isOpen) {
    return null;
  }

  return (
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
          value={props.filters.dateRange}
          onChange={(dateRange: DatesRangeValue<string>) => {
            const parsedDateRange: [Date | null, Date | null] = [
              dateRange[0] ? dayjs(dateRange[0]).toDate() : null,
              dateRange[1] ? dayjs(dateRange[1]).toDate() : null,
            ];
            props.setFilters({
              ...props.filters,
              dateRange: parsedDateRange,
            });
          }}
        />
        <AccountSelectInput
          w={{ base: "100%", sm: "50%" }}
          selectedAccountIds={props.filters.accounts}
          setSelectedAccountIds={(newAccountIds: string[]) => {
            props.setFilters({
              ...props.filters,
              accounts: newAccountIds,
            });
          }}
          hideHidden
        />
        <CategorySelect
          w={{ base: "100%", sm: "20%" }}
          categories={props.categories}
          value={props.filters.category}
          onChange={(val) =>
            props.setFilters({ ...props.filters, category: val })
          }
          withinPortal
          includeUncategorized
        />
        <Button
          w={{ base: "100%", sm: "130px" }}
          onClick={() => {
            props.setFilters({
              dateRange: [null, null],
              accounts: [],
              category: "",
            });
          }}
        >
          Clear Filters
        </Button>
      </Flex>
    </Stack>
  );
};

export default FilterCard;
