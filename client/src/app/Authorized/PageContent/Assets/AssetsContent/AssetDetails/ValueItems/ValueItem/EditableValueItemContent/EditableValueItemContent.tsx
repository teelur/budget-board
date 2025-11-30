import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import dayjs from "dayjs";
import { PencilIcon, Trash2Icon, Undo2Icon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IValueResponse, IValueUpdateRequest } from "~/models/value";
import ElevatedDateInput from "~/components/Input/Elevated/ElevatedDateInput/ElevatedDateInput";
import ElevatedNumberInput from "~/components/Input/Elevated/ElevatedNumberInput/ElevatedNumberInput";

interface EditableValueItemContentProps {
  value: IValueResponse;
  userCurrency: string;
  doUnSelect: () => void;
}

const EditableValueItemContent = (
  props: EditableValueItemContentProps
): React.ReactNode => {
  const valueAmountField = useField<string | number | undefined>({
    initialValue: props.value.amount,
    validateOnBlur: true,
    validate: (value) => {
      if (value === undefined || value === null || isNaN(Number(value))) {
        return "Amount must be a valid number";
      }
      return null;
    },
  });
  const valueDateField = useField<string>({
    initialValue: dayjs(props.value.dateTime).format("YYYY-MM-DD"),
    validateOnBlur: true,
    validate: (value) => {
      if (!dayjs(value).isValid()) {
        return "Date must be valid";
      }
      return null;
    },
  });

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doUpdateValue = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/value`,
        method: "PUT",
        data: {
          id: props.value.id,
          amount: Number(valueAmountField.getValue()),
          dateTime: dayjs(valueDateField.getValue()).toDate(),
        } as IValueUpdateRequest,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["assets"],
      });
      await queryClient.invalidateQueries({
        queryKey: ["values", props.value.assetID],
      });

      notifications.show({ color: "green", message: "Value updated" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  const doDeleteValue = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/value`,
        method: "DELETE",
        params: { guid: props.value.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["assets"],
      });
      await queryClient.invalidateQueries({
        queryKey: ["values", props.value.assetID],
      });

      notifications.show({ color: "green", message: "Value deleted" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  const doRestoreValue = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/value/restore`,
        method: "POST",
        params: { guid: props.value.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["assets"],
      });
      await queryClient.invalidateQueries({
        queryKey: ["values", props.value.assetID],
      });

      notifications.show({ color: "green", message: "Value restored" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  useDidUpdate(() => {
    doUpdateValue.mutate();
  }, [valueDateField.getValue()]);

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <LoadingOverlay
        visible={
          doUpdateValue.isPending ||
          doDeleteValue.isPending ||
          doRestoreValue.isPending
        }
      />
      <Stack w="100%">
        <ElevatedDateInput
          {...valueDateField.getInputProps()}
          flex="1 1 auto"
        />
        <ElevatedNumberInput
          {...valueAmountField.getInputProps()}
          flex="1 1 auto"
          prefix={getCurrencySymbol(props.userCurrency)}
          thousandSeparator=","
          decimalScale={2}
          fixedDecimalScale
          onBlur={() => doUpdateValue.mutate()}
        />
      </Stack>
      <Group style={{ alignSelf: "stretch" }} gap="0.5rem" wrap="nowrap">
        <ActionIcon
          h="100%"
          variant="outline"
          size="md"
          onClick={(e) => {
            e.stopPropagation();
            props.doUnSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
        {props.value.deleted ? (
          <ActionIcon
            h="100%"
            size="sm"
            onClick={() => doRestoreValue.mutate()}
          >
            <Undo2Icon size={16} />
          </ActionIcon>
        ) : (
          <ActionIcon
            h="100%"
            size="sm"
            bg="var(--button-color-destructive)"
            onClick={() => doDeleteValue.mutate()}
          >
            <Trash2Icon size={16} />
          </ActionIcon>
        )}
      </Group>
    </Group>
  );
};

export default EditableValueItemContent;
