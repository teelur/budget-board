import {
  ActionIcon,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  Stack,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { PencilIcon } from "lucide-react";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { IAccountResponse } from "~/models/account";
import DeleteAccountPopover from "./DeleteAccountPopover/DeleteAccountPopover";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import { useTranslation } from "react-i18next";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useUpdateAccountMutation } from "~/hooks/mutations/accounts/useUpdateAccountMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface EditableAccountItemContentProps {
  account: IAccountResponse;
  toggle: () => void;
}

const EditableAccountItemContent = (props: EditableAccountItemContentProps) => {
  const { t } = useTranslation();
  const {
    dayjs,
    dateFormat,
    intlLocale,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { allAccountTypes } = useAccountTypes();
  const updateAccountMutation = useUpdateAccountMutation();

  const accountNameField = useField<string>({
    initialValue: props.account.name,
    validateOnBlur: true,
    validate: (value) => {
      if (value.trim().length === 0) {
        return "Account name cannot be empty";
      }
      return null;
    },
  });

  const interestRateField = useField<number | string | undefined>({
    initialValue: props.account.interestRate
      ? props.account.interestRate * 100
      : undefined,
  });

  const accountTypeField = useField<string>({
    initialValue: props.account.type,
  });

  const hideAccountField = useField<boolean>({
    initialValue: props.account.hideAccount ?? false,
  });

  const hideTransactionsField = useField<boolean>({
    initialValue: props.account.hideTransactions ?? false,
  });

  const resetFormToServerValues = () => {
    accountNameField.setValue(props.account.name);
    interestRateField.setValue(
      props.account.interestRate ? props.account.interestRate * 100 : undefined,
    );
    accountTypeField.setValue(props.account.type);
    hideAccountField.setValue(props.account.hideAccount ?? false);
    hideTransactionsField.setValue(props.account.hideTransactions ?? false);
  };

  useDidUpdate(() => {
    updateAccountMutation.mutate(
      {
        id: props.account.id,
        type: accountTypeField.getValue(),
        hideAccount: hideAccountField.getValue(),
        hideTransactions: hideTransactionsField.getValue(),
      },
      {
        onError: resetFormToServerValues,
      },
    );
  }, [
    accountTypeField.getValue(),
    hideAccountField.getValue(),
    hideTransactionsField.getValue(),
  ]);

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <Stack gap="0.5rem" flex="1 1 auto">
        <LoadingOverlay visible={updateAccountMutation.isPending} />
        <Group justify="space-between" align="flex-end">
          <Group gap="0.5rem" align="flex-end">
            <TextInput
              {...accountNameField.getInputProps()}
              label={<PrimaryText size="xs">{t("name")}</PrimaryText>}
              onBlur={() =>
                updateAccountMutation.mutate(
                  {
                    id: props.account.id,
                    name: accountNameField.getValue(),
                  },
                  {
                    onError: resetFormToServerValues,
                  },
                )
              }
              elevation={2}
            />
            <Flex style={{ alignSelf: "stretch" }}>
              <ActionIcon
                variant="outline"
                h="100%"
                size="md"
                onClick={(e) => {
                  e.stopPropagation();
                  props.toggle();
                }}
              >
                <PencilIcon size={16} />
              </ActionIcon>
            </Flex>
            <NumberInput
              {...interestRateField.getInputProps()}
              label={<PrimaryText size="xs">{t("interest_rate")}</PrimaryText>}
              decimalScale={2}
              thousandSeparator={thousandsSeparator}
              decimalSeparator={decimalSeparator}
              min={0}
              step={1}
              suffix="%"
              maw={90}
              onBlur={() =>
                updateAccountMutation.mutate(
                  {
                    id: props.account.id,
                    interestRate:
                      ((interestRateField.getValue() ?? 0) as number) / 100,
                  },
                  {
                    onError: resetFormToServerValues,
                  },
                )
              }
              elevation={2}
            />
            <Group gap="0.5rem">
              <Button
                bg={
                  hideAccountField.getValue()
                    ? "var(--button-color-warning)"
                    : undefined
                }
                variant={hideAccountField.getValue() ? "filled" : "outline"}
                onClick={() =>
                  hideAccountField.setValue(!hideAccountField.getValue())
                }
              >
                {t("hide_account")}
              </Button>
              <Button
                bg={
                  hideTransactionsField.getValue()
                    ? "var(--accent-color-purple)"
                    : undefined
                }
                variant={
                  hideTransactionsField.getValue() ? "filled" : "outline"
                }
                onClick={() =>
                  hideTransactionsField.setValue(
                    !hideTransactionsField.getValue(),
                  )
                }
              >
                {t("hide_transactions")}
              </Button>
            </Group>
          </Group>
          <StatusText amount={props.account.currentBalance} size="md">
            {convertNumberToCurrency(
              props.account.currentBalance,
              true,
              preferredCurrency,
              SignDisplay.Auto,
              intlLocale,
            )}
          </StatusText>
        </Group>
        <Group justify="space-between" align="center">
          <CategorySelect
            w={220}
            categories={allAccountTypes}
            value={accountTypeField.getValue()}
            onChange={(val: string) => {
              accountTypeField.setValue(val);
            }}
            withinPortal
            elevation={2}
          />
          <DimmedText size="sm">
            {t("last_updated", {
              date: dayjs(props.account.balanceDate).isValid()
                ? dayjs(props.account.balanceDate).format(`${dateFormat}`)
                : t("never"),
            })}
          </DimmedText>
        </Group>
      </Stack>
      <Group style={{ alignSelf: "stretch" }}>
        <DeleteAccountPopover accountId={props.account.id} />
      </Group>
    </Group>
  );
};

export default EditableAccountItemContent;
