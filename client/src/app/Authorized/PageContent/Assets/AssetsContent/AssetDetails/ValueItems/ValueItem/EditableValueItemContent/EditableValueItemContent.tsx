import { ActionIcon, Group, NumberInput } from "@mantine/core";
import { DatePickerInput } from "@mantine/dates";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IValueResponse, IValueUpdateRequest } from "~/models/value";

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

  const { request } = React.useContext<any>(AuthContext);

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
        queryKey: ["values", props.value.assetID],
      });

      notifications.show({ color: "green", message: "Value updated" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  useDidUpdate(() => {
    doUpdateValue.mutate();
  }, [valueDateField.getValue()]);

  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <DatePickerInput {...valueDateField.getInputProps()} />
        <ActionIcon
          variant="outline"
          size="md"
          onClick={(e) => {
            e.stopPropagation();
            props.doUnSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <NumberInput
        {...valueAmountField.getInputProps()}
        prefix={getCurrencySymbol(props.userCurrency)}
        thousandSeparator=","
        decimalScale={2}
        fixedDecimalScale
        onBlur={() => doUpdateValue.mutate()}
        maw={145}
      />
    </Group>
  );
};

export default EditableValueItemContent;
