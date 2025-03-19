import CategorySelect from "~/components/CategorySelect";
import classes from "./FilterCard.module.css";

import AccountSelectInput from "~/components/AccountSelectInput";
import { Card, Flex, Stack, Title } from "@mantine/core";
import { DatePickerInput } from "@mantine/dates";
import { Filters } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";

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
    <Card className={classes.root} radius="md">
      <Stack gap="0.5rem">
        <Title order={3}>Filters</Title>
        <Flex
          className={classes.group}
          direction={{ base: "column", sm: "row" }}
          wrap="nowrap"
          gap="md"
        >
          <DatePickerInput
            w={{ base: "100%", sm: "30%" }}
            type="range"
            placeholder="Pick a date range"
            value={props.filters.dateRange}
            onChange={(dateRange: [Date | null, Date | null]) =>
              props.setFilters({
                ...props.filters,
                dateRange,
              })
            }
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
          />
        </Flex>
      </Stack>
    </Card>
  );
};

export default FilterCard;
