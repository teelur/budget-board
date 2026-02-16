import { Flex, Stack, Button } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import { Filters } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import DatePickerInput from "~/components/core/Input/DatePickerInput/DatePickerInput";
import { useTranslation } from "react-i18next";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface FilterCardProps {
  categories: ICategory[];
  style?: React.CSSProperties;
}

const FilterCard = (props: FilterCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, locale, longDateFormat } = useDate();
  const { transactionFilters, setTransactionFilters } = useTransactionFilters();

  return (
    <Card elevation={1} style={props.style}>
      <Stack gap={0}>
        <PrimaryText size="lg">{t("filters")}</PrimaryText>
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
            placeholder={t("select_a_date_range")}
            value={transactionFilters.dateRange}
            locale={locale}
            valueFormat={longDateFormat}
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
            label={<PrimaryText size="sm">{t("date_range")}</PrimaryText>}
            elevation={1}
          />
          <AccountMultiSelect
            w={{ base: "100%", sm: "50%" }}
            value={transactionFilters.accounts}
            onChange={(newAccountIds: string[]) => {
              const newFilters = new Filters();
              newFilters.accounts = newAccountIds;
              newFilters.category = transactionFilters.category;
              newFilters.dateRange = transactionFilters.dateRange;
              setTransactionFilters(newFilters);
            }}
            hideHidden
            label={<PrimaryText size="sm">{t("accounts")}</PrimaryText>}
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
            label={<PrimaryText size="sm">{t("category")}</PrimaryText>}
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
            {t("clear_filters")}
          </Button>
        </Flex>
      </Stack>
    </Card>
  );
};

export default FilterCard;
