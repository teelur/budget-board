import React from "react";
import { Stack } from "@mantine/core";
import {
  ActionIcon,
  Button,
  CategorySelect,
  NumberInput,
  Popover,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import { SplitIcon } from "lucide-react";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { ICategory } from "~/models/category";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useSplitTransactionMutation } from "~/hooks/mutations/transactions/useSplitTransactionMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface SplitTransactionProps {
  id: string;
  originalAmount: number;
  categories: ICategory[];
  elevation: number;
}

const SplitTransaction = (props: SplitTransactionProps): React.ReactNode => {
  const amountField = useField<string | number>({
    initialValue: 0,
  });

  const categoryField = useField<string>({
    initialValue: "",
  });

  const { t } = useTranslation();
  const { thousandsSeparator, decimalSeparator } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const splitTransactionMutation = useSplitTransactionMutation();

  return (
    <Popover>
      <Popover.Target>
        <ActionIcon variant="filled" color="primary" size="compact-sm">
          <SplitIcon size="1rem" />
        </ActionIcon>
      </Popover.Target>
      <Popover.Dropdown>
        <Stack gap="0.5rem">
          <NumberInput
            label={t("amount")}
            {...amountField.getInputProps()}
            prefix={getCurrencySymbol(preferredCurrency)}
            decimalScale={decimalPlaces}
            thousandSeparator={thousandsSeparator}
            decimalSeparator={decimalSeparator}
            maw={200}
          />
          <CategorySelect
            label={t("category")}
            {...categoryField.getInputProps()}
            categories={props.categories}
          />
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            loading={splitTransactionMutation.isPending}
            onClick={() => {
              splitTransactionMutation.mutate({
                id: props.id,
                amount:
                  amountField.getValue() === ""
                    ? 0
                    : (amountField.getValue() as number),
                category: getParentCategory(
                  categoryField.getValue(),
                  props.categories,
                ),
                subcategory: getIsParentCategory(
                  categoryField.getValue(),
                  props.categories,
                )
                  ? ""
                  : categoryField.getValue(),
              });
            }}
          >
            {t("split_transaction")}
          </Button>
        </Stack>
      </Popover.Dropdown>
    </Popover>
  );
};

export default SplitTransaction;
