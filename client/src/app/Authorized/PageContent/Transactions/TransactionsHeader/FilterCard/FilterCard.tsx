import classes from "./FilterCard.module.css";

import { Flex, Stack, Button } from "@mantine/core";
import { DatesRangeValue } from "@mantine/dates";
import { Filters } from "~/models/transaction";
import React from "react";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import DatePickerInput from "~/components/core/Input/DatePickerInput/DatePickerInput";
import { useTranslation } from "react-i18next";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import useTransactionCategories from "~/hooks/useTransactionCategories";

const FilterCard = (): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dayjsLocale, longDateFormat } = useLocale();
  const { transactionFilters, setTransactionFilters } = useTransactionFilters();
  const { data: transactionCategories = [] } = useTransactionCategories();

  return (
    <Card elevation={1}>
      <Stack gap={0} className={classes.root}>
        <PrimaryText size="lg">{t("filters")}</PrimaryText>
        <Flex
          className={classes.container}
          justify="space-between"
          wrap="nowrap"
        >
          <DatePickerInput
            className={classes.datePickerInput}
            miw={165}
            type="range"
            label={<PrimaryText size="sm">{t("date_range")}</PrimaryText>}
            placeholder={t("select_a_date_range")}
            value={transactionFilters.dateRange}
            locale={dayjsLocale}
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
            elevation={1}
          />
          <AccountMultiSelect
            className={classes.accountMultiSelect}
            miw={150}
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
            className={classes.categorySelect}
            miw={170}
            categories={transactionCategories}
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
            className={classes.clearButton}
            w="100%"
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
