import classes from "./FilterCard.module.css";

import { Flex, Stack, Button, Group } from "@mantine/core";
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
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";

const FilterCard = (): React.ReactNode => {
  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
    currencySymbol,
  } = useLocale();
  const { transactionFilters, setTransactionFilters } = useTransactionFilters();
  const { transactionCategories } = useTransactionCategories();

  return (
    <Card elevation={1}>
      <Stack gap="0.25rem" className={classes.root}>
        <Flex
          className={classes.header}
          justify="space-between"
          align="center"
          wrap="nowrap"
        >
          <PrimaryText size="lg">{t("filters")}</PrimaryText>
          <Button
            className={classes.clearButton}
            w="100%"
            size="xs"
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
        <Flex className={classes.row} justify="space-between" wrap="nowrap">
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
              newFilters.merchantName = transactionFilters.merchantName;
              newFilters.amountRange = transactionFilters.amountRange;
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
              newFilters.merchantName = transactionFilters.merchantName;
              newFilters.amountRange = transactionFilters.amountRange;
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
              newFilters.merchantName = transactionFilters.merchantName;
              newFilters.amountRange = transactionFilters.amountRange;
              setTransactionFilters(newFilters);
            }}
            withinPortal
            includeUncategorized
            label={<PrimaryText size="sm">{t("category")}</PrimaryText>}
            elevation={1}
          />
        </Flex>
        <Flex className={classes.row} justify="space-between" wrap="nowrap">
          <TextInput
            className={classes.merchantInput}
            miw={140}
            label={<PrimaryText size="sm">{t("merchant_name")}</PrimaryText>}
            placeholder={t("enter_merchant_name")}
            value={transactionFilters.merchantName}
            onChange={(e) => {
              const newFilters = new Filters();
              newFilters.accounts = transactionFilters.accounts;
              newFilters.category = transactionFilters.category;
              newFilters.dateRange = transactionFilters.dateRange;
              newFilters.merchantName = e.currentTarget.value;
              newFilters.amountRange = transactionFilters.amountRange;
              setTransactionFilters(newFilters);
            }}
            elevation={1}
          />
          <NumberInput
            className={classes.amountInput}
            miw={100}
            label={<PrimaryText size="sm">{t("amount_min")}</PrimaryText>}
            placeholder="0"
            value={transactionFilters.amountRange[0] ?? ""}
            onChange={(val) => {
              const newFilters = new Filters();
              newFilters.accounts = transactionFilters.accounts;
              newFilters.category = transactionFilters.category;
              newFilters.dateRange = transactionFilters.dateRange;
              newFilters.merchantName = transactionFilters.merchantName;
              newFilters.amountRange = [
                val === "" ? null : Number(val),
                transactionFilters.amountRange[1],
              ];
              setTransactionFilters(newFilters);
            }}
            prefix={currencySymbol}
            decimalScale={2}
            decimalSeparator={decimalSeparator}
            thousandSeparator={thousandsSeparator}
            elevation={1}
          />
          <NumberInput
            className={classes.amountInput}
            miw={100}
            label={<PrimaryText size="sm">{t("amount_max")}</PrimaryText>}
            placeholder="0"
            value={transactionFilters.amountRange[1] ?? ""}
            onChange={(val) => {
              const newFilters = new Filters();
              newFilters.accounts = transactionFilters.accounts;
              newFilters.category = transactionFilters.category;
              newFilters.dateRange = transactionFilters.dateRange;
              newFilters.merchantName = transactionFilters.merchantName;
              newFilters.amountRange = [
                transactionFilters.amountRange[0],
                val === "" ? null : Number(val),
              ];
              setTransactionFilters(newFilters);
            }}
            prefix={currencySymbol}
            decimalScale={2}
            decimalSeparator={decimalSeparator}
            thousandSeparator={thousandsSeparator}
            elevation={1}
          />
        </Flex>
      </Stack>
    </Card>
  );
};

export default FilterCard;
