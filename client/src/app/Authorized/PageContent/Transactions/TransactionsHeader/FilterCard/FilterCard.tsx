import { Flex, Stack, Button } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import { Filters } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import dayjs from "dayjs";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import Card from "~/components/Card/Card";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import CategorySelect from "~/components/Select/CategorySelect/CategorySelect";
import AccountSelect from "~/components/Select/AccountSelect/AccountSelect";
import DatePickerInput from "~/components/Input/DatePickerInput/DatePickerInput";

interface FilterCardProps {
  categories: ICategory[];
  style?: React.CSSProperties;
}

const FilterCard = (props: FilterCardProps): React.ReactNode => {
  const { transactionFilters, setTransactionFilters } = useTransactionFilters();

  return (
    <Card elevation={1} style={props.style}>
      <Stack gap={0}>
        <PrimaryText size="lg">Filters</PrimaryText>
        <Flex
          justify="space-between"
          align="center"
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
            label={<PrimaryText size="sm">Date Range</PrimaryText>}
            elevation={1}
          />
          <AccountSelect
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
            label={<PrimaryText size="sm">Accounts</PrimaryText>}
            elevation={1}
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
            label={<PrimaryText size="sm">Category</PrimaryText>}
            elevation={1}
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
