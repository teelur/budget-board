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
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PencilIcon } from "lucide-react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { convertNumberToCurrency } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import {
  accountCategories,
  IAccountResponse,
  IAccountUpdateRequest,
} from "~/models/account";
import DeleteAccountPopover from "./DeleteAccountPopover/DeleteAccountPopover";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import ElevatedNumberInput from "~/components/core/Input/Elevated/ElevatedNumberInput/ElevatedNumberInput";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import { useTranslation } from "react-i18next";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface EditableAccountItemContentProps {
  account: IAccountResponse;
  userCurrency: string;
  toggle: () => void;
}

const EditableAccountItemContent = (props: EditableAccountItemContentProps) => {
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

  const accountSubTypeField = useField<string>({
    initialValue: props.account.subtype ?? "",
  });

  const hideAccountField = useField<boolean>({
    initialValue: props.account.hideAccount ?? false,
  });

  const hideTransactionsField = useField<boolean>({
    initialValue: props.account.hideTransactions ?? false,
  });

  const { t } = useTranslation();
  const { dayjs, dateFormat } = useDate();
  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doUpdateAccount = useMutation({
    mutationFn: async () => {
      const editedAccount: IAccountUpdateRequest = {
        id: props.account.id,
        name: accountNameField.getValue(),
        type: accountTypeField.getValue(),
        subtype: accountSubTypeField.getValue(),
        hideTransactions: hideTransactionsField.getValue(),
        hideAccount: hideAccountField.getValue(),
        interestRate: ((interestRateField.getValue() ?? 0) as number) / 100,
      };

      return await request({
        url: "/api/account",
        method: "PUT",
        data: editedAccount,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });

      // Reset fields to original values on error
      accountNameField.setValue(props.account.name);
      interestRateField.setValue(
        props.account.interestRate
          ? props.account.interestRate * 100
          : undefined,
      );
      accountTypeField.setValue(props.account.type);
      accountSubTypeField.setValue(props.account.subtype ?? "");
      hideAccountField.setValue(props.account.hideAccount ?? false);
      hideTransactionsField.setValue(props.account.hideTransactions ?? false);
    },
  });

  useDidUpdate(
    () => doUpdateAccount.mutate(),
    [
      accountTypeField.getValue(),
      accountSubTypeField.getValue(),
      hideAccountField.getValue(),
      hideTransactionsField.getValue(),
    ],
  );

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <Stack gap="0.5rem" flex="1 1 auto">
        <LoadingOverlay visible={doUpdateAccount.isPending} />
        <Group justify="space-between" align="flex-end">
          <Group gap="0.5rem" align="flex-end">
            <TextInput
              {...accountNameField.getInputProps()}
              label={<PrimaryText size="xs">{t("name")}</PrimaryText>}
              onBlur={() => doUpdateAccount.mutate()}
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
            <ElevatedNumberInput
              {...interestRateField.getInputProps()}
              label={<PrimaryText size="xs">{t("interest_rate")}</PrimaryText>}
              decimalScale={2}
              min={0}
              step={1}
              suffix="%"
              maw={90}
              onBlur={() => doUpdateAccount.mutate()}
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
                bg={hideTransactionsField.getValue() ? "purple" : undefined}
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
              props.userCurrency,
            )}
          </StatusText>
        </Group>
        <Group justify="space-between" align="center">
          <CategorySelect
            w={220}
            categories={accountCategories}
            value={
              accountSubTypeField.getValue().length > 0
                ? accountSubTypeField.getValue()
                : accountTypeField.getValue()
            }
            onChange={(val: string) => {
              const parent = getParentCategory(val, accountCategories);
              accountTypeField.setValue(parent);
              getIsParentCategory(val, accountCategories)
                ? accountSubTypeField.setValue("")
                : accountSubTypeField.setValue(val);
            }}
            withinPortal
            elevation={2}
          />
          <DimmedText size="sm">
            {t("last_updated", {
              date: dayjs(props.account.balanceDate).isValid()
                ? dayjs(props.account.balanceDate).format(`${dateFormat} LT`)
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
