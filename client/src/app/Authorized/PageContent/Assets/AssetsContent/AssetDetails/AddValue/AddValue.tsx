import { Button, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getCurrencySymbol } from "~/helpers/currency";
import { IValueCreateRequest, IValueResponse } from "~/models/value";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SurfaceDateInput from "~/components/core/Input/Surface/SurfaceDateInput/SurfaceDateInput";
import SurfaceNumberInput from "~/components/core/Input/Surface/SurfaceNumberInput/SurfaceNumberInput";

interface AddValueProps {
  assetId: string;
  currency: string;
}

const AddValue = (props: AddValueProps): React.ReactNode => {
  const amountField = useField<string | number>({
    initialValue: 0,
  });
  const dateField = useField<string>({
    initialValue: dayjs().toString(),
  });

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doAddValue = useMutation({
    mutationFn: async (newValue: IValueCreateRequest) => {
      const res = await request({
        url: `/api/value`,
        method: "POST",
        data: newValue,
      });

      if (res.status === 200) {
        return res.data as IValueResponse;
      }

      return undefined;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assets"] });
      queryClient.invalidateQueries({ queryKey: ["values", props.assetId] });

      amountField.reset();
      dateField.reset();

      notifications.show({ color: "green", message: "Value added" });
    },
  });

  return (
    <Stack gap={10}>
      <SurfaceDateInput
        {...dateField.getInputProps()}
        label={<PrimaryText size="xs">Date</PrimaryText>}
        maw={400}
      />
      <SurfaceNumberInput
        {...amountField.getInputProps()}
        label={<PrimaryText size="xs">Amount</PrimaryText>}
        prefix={getCurrencySymbol(props.currency)}
        decimalScale={2}
        thousandSeparator=","
      />
      <Button
        loading={doAddValue.isPending}
        onClick={() =>
          doAddValue.mutate({
            amount: Number(amountField.getValue()),
            dateTime: dayjs(dateField.getValue()).toDate(),
            assetID: props.assetId,
          })
        }
      >
        Add Value
      </Button>
    </Stack>
  );
};

export default AddValue;
