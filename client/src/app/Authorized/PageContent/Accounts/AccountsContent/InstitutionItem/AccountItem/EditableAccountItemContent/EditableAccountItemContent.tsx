import {
  ActionIcon,
  Button,
  Group,
  LoadingOverlay,
  NumberInput,
  Stack,
  Text,
  TextInput,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import dayjs from "dayjs";
import { PencilIcon, Undo2Icon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import CategorySelect from "~/components/CategorySelect";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { convertNumberToCurrency } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import {
  accountCategories,
  IAccountResponse,
  IAccountUpdateRequest,
} from "~/models/account";
import DeleteAccountPopover from "./DeleteAccountPopover/DeleteAccountPopover";

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

  const { request } = React.useContext<any>(AuthContext);

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
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  const doRestoreAccount = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/account/restore`,
        method: "POST",
        params: { guid: props.account.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });

  useDidUpdate(
    () => doUpdateAccount.mutate(),
    [
      accountTypeField.getValue(),
      accountSubTypeField.getValue(),
      hideAccountField.getValue(),
      hideTransactionsField.getValue(),
    ]
  );

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <Stack gap="0.5rem" flex="1 1 auto">
        <LoadingOverlay
          visible={doUpdateAccount.isPending || doRestoreAccount.isPending}
        />
        <Group justify="space-between" align="center">
          <Group gap="0.5rem" align="center">
            <TextInput
              {...accountNameField.getInputProps()}
              label={
                <Text fw={600} size="xs" c="dimmed">
                  Name
                </Text>
              }
            />
            <ActionIcon
              variant="outline"
              size="md"
              onClick={(e) => {
                e.stopPropagation();
                props.toggle();
              }}
            >
              <PencilIcon size={16} />
            </ActionIcon>
            <NumberInput
              {...interestRateField.getInputProps()}
              label={
                <Text fw={600} size="xs" c="dimmed">
                  Interest Rate
                </Text>
              }
              decimalScale={2}
              min={0}
              step={1}
              suffix="%"
              maw={90}
            />
            <Group gap="0.5rem">
              <Button
                bg={hideAccountField.getValue() ? "yellow" : undefined}
                variant={hideAccountField.getValue() ? "filled" : "outline"}
                onClick={() =>
                  hideAccountField.setValue(!hideAccountField.getValue())
                }
              >
                Hide Account
              </Button>
              <Button
                variant={
                  hideTransactionsField.getValue() ? "filled" : "outline"
                }
                onClick={() =>
                  hideTransactionsField.setValue(
                    !hideTransactionsField.getValue()
                  )
                }
              >
                Hide Transactions
              </Button>
            </Group>
          </Group>
          <Text
            fw={600}
            size="md"
            c={props.account.currentBalance < 0 ? "red" : "green"}
          >
            {convertNumberToCurrency(
              props.account.currentBalance,
              true,
              props.userCurrency
            )}
          </Text>
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
            onChange={(val) => {
              const parent = getParentCategory(val, accountCategories);
              accountTypeField.setValue(parent);
              getIsParentCategory(val, accountCategories)
                ? accountSubTypeField.setValue("")
                : accountSubTypeField.setValue(val);
            }}
            withinPortal
          />
          <Text fw={600} size="sm" c="dimmed">
            Last Updated: {dayjs(props.account.balanceDate).format("L LT")}
          </Text>
        </Group>
      </Stack>
      <Group style={{ alignSelf: "stretch" }}>
        {props.account.deleted ? (
          <ActionIcon
            h="100%"
            size="sm"
            onClick={() => doRestoreAccount.mutate()}
          >
            <Undo2Icon size={16} />
          </ActionIcon>
        ) : (
          <DeleteAccountPopover accountId={props.account.id} />
        )}
      </Group>
    </Group>
  );
};

export default EditableAccountItemContent;
