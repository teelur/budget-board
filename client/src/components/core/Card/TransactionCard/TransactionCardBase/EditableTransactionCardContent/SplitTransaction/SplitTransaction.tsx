import React from "react";
import {
  ActionIcon,
  Button,
  Stack,
  Popover as MantinePopover,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { SplitIcon } from "lucide-react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { ICategory } from "~/models/category";
import { ITransactionSplitRequest } from "~/models/transaction";
import { IUserSettings } from "~/models/userSettings";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import Popover from "~/components/core/Popover/Popover";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

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
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doSplitTransaction = useMutation({
    mutationFn: async (splitTransaction: ITransactionSplitRequest) =>
      await request({
        url: "/api/transaction/split",
        method: "POST",
        data: splitTransaction,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      notifications.show({
        message: t("transaction_split_successfully"),
        color: "var(--button-color-confirm)",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  return (
    <Popover>
      <MantinePopover.Target>
        <ActionIcon h="100%">
          <SplitIcon size="1rem" />
        </ActionIcon>
      </MantinePopover.Target>
      <MantinePopover.Dropdown style={{ padding: "0.5rem" }}>
        <Stack gap="0.5rem">
          <NumberInput
            label={<PrimaryText size="sm">{t("amount")}</PrimaryText>}
            {...amountField.getInputProps()}
            prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
            decimalScale={2}
            thousandSeparator={thousandsSeparator}
            decimalSeparator={decimalSeparator}
            maw={200}
            elevation={props.elevation}
          />
          <CategorySelect
            label={<PrimaryText size="sm">{t("category")}</PrimaryText>}
            {...categoryField.getInputProps()}
            categories={props.categories}
            elevation={props.elevation}
          />
          <Button
            size="compact-sm"
            loading={doSplitTransaction.isPending}
            onClick={() => {
              doSplitTransaction.mutate({
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
      </MantinePopover.Dropdown>
    </Popover>
  );
};

export default SplitTransaction;
