import { ActionIcon, Group, LoadingOverlay, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { PencilIcon, Trash2Icon } from "lucide-react";
import React from "react";
import { getCurrencySymbol } from "~/helpers/currency";
import { IValueResponse } from "~/models/value";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useUpdateValueMutation } from "~/hooks/mutations/values/useUpdateValueMutation";
import { useDeleteValueMutation } from "~/hooks/mutations/values/useDeleteValueMutation";

interface EditableValueItemContentProps {
  value: IValueResponse;
  userCurrency: string;
  doUnSelect: () => void;
}

const EditableValueItemContent = (
  props: EditableValueItemContentProps,
): React.ReactNode => {
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const updateValueMutation = useUpdateValueMutation({
    assetId: props.value.assetID,
  });
  const deleteValueMutation = useDeleteValueMutation({
    assetId: props.value.assetID,
  });

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
  const valueDateField = useField<Date>({
    initialValue: dayjs(props.value.date).toDate(),
  });

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <LoadingOverlay
        visible={updateValueMutation.isPending || deleteValueMutation.isPending}
      />
      <Stack w="100%">
        <DateInput
          {...valueDateField.getInputProps()}
          flex="1 1 auto"
          locale={dayjsLocale}
          valueFormat={longDateFormat}
          onChange={(date) =>
            updateValueMutation.mutate({
              id: props.value.id,
              date: dayjs(date).isValid()
                ? dayjs(date).format("YYYY-MM-DD")
                : undefined,
            })
          }
          elevation={2}
        />
        <NumberInput
          {...valueAmountField.getInputProps()}
          flex="1 1 auto"
          prefix={getCurrencySymbol(props.userCurrency)}
          thousandSeparator={thousandsSeparator}
          decimalSeparator={decimalSeparator}
          decimalScale={2}
          fixedDecimalScale
          onBlur={() =>
            updateValueMutation.mutate({
              id: props.value.id,
              amount: Number(valueAmountField.getValue()),
            })
          }
          elevation={2}
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
        <ActionIcon
          h="100%"
          size="sm"
          bg="var(--button-color-destructive)"
          onClick={() => deleteValueMutation.mutate(props.value.id)}
        >
          <Trash2Icon size={16} />
        </ActionIcon>
      </Group>
    </Group>
  );
};

export default EditableValueItemContent;
