import {
  ActionIcon,
  Group,
  Text,
  Stack,
  TextInput,
  LoadingOverlay,
  Button,
  NumberInput,
} from "@mantine/core";
import { DatePickerInput } from "@mantine/dates";
import { useField } from "@mantine/form";
import { useDidUpdate } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import dayjs from "dayjs";
import { PencilIcon, Trash2Icon, Undo2Icon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { convertNumberToCurrency, getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IAssetResponse, IAssetUpdateRequest } from "~/models/asset";

interface EditableAssetItemContentProps {
  asset: IAssetResponse;
  userCurrency: string;
  toggle: () => void;
}

const EditableAssetItemContent = (
  props: EditableAssetItemContentProps
): React.ReactNode => {
  const assetNameField = useField<string>({
    initialValue: props.asset.name,
    validateOnBlur: true,
    validate: (value) => {
      if (value.trim().length === 0) {
        return "Asset name cannot be empty";
      }
      return null;
    },
  });
  const purchaseDate = useField<string | null>({
    initialValue: props.asset.purchaseDate
      ? dayjs(props.asset.purchaseDate).toString()
      : null,
  });
  const purchasePrice = useField<string | number | undefined>({
    initialValue: props.asset.purchasePrice ?? undefined,
  });
  const sellDate = useField<string | null>({
    initialValue: props.asset.sellDate
      ? dayjs(props.asset.sellDate).toString()
      : null,
  });
  const sellPrice = useField<string | number | undefined>({
    initialValue: props.asset.sellPrice ?? undefined,
  });
  const hideAssetField = useField<boolean>({
    initialValue: props.asset.hide,
  });

  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doUpdateAsset = useMutation({
    mutationFn: async () => {
      const editedAsset: IAssetUpdateRequest = {
        id: props.asset.id,
        name: assetNameField.getValue(),
        purchaseDate: dayjs(purchaseDate.getValue()).toDate() || null,
        purchasePrice:
          purchasePrice.getValue() === undefined
            ? null
            : Number(purchasePrice.getValue()),
        sellDate: dayjs(sellDate.getValue()).toDate() || null,
        sellPrice:
          sellPrice.getValue() === undefined
            ? null
            : Number(sellPrice.getValue()),
        hide: hideAssetField.getValue(),
      };

      return await request({
        url: "/api/asset",
        method: "PUT",
        data: editedAsset,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["assets"] });

      notifications.show({ color: "green", message: "Asset updated" });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });

      // Reset fields to original values on error
      assetNameField.setValue(props.asset.name);
    },
  });

  const doDeleteAsset = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/asset`,
        method: "DELETE",
        params: { guid: props.asset.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["assets"] });

      notifications.show({ color: "green", message: "Asset deleted" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  const doRestoreAsset = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/asset/restore`,
        method: "POST",
        params: { guid: props.asset.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["assets"] });

      notifications.show({ color: "green", message: "Asset restored" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  useDidUpdate(
    () => doUpdateAsset.mutate(),
    [purchaseDate.getValue(), sellDate.getValue(), hideAssetField.getValue()]
  );

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <Stack gap="0.5rem" flex="1 1 auto">
        <LoadingOverlay visible={doUpdateAsset.isPending} />
        <Group justify="space-between" align="center">
          <Group gap="0.5rem" align="center">
            <TextInput
              {...assetNameField.getInputProps()}
              onBlur={() => doUpdateAsset.mutate()}
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
            <Button
              bg={hideAssetField.getValue() ? "yellow" : undefined}
              variant={hideAssetField.getValue() ? "filled" : "outline"}
              onClick={() =>
                hideAssetField.setValue(!hideAssetField.getValue())
              }
            >
              Hide Asset
            </Button>
          </Group>
          <Text
            fw={600}
            size="md"
            c={props.asset.currentValue < 0 ? "red" : "green"}
          >
            {convertNumberToCurrency(
              props.asset.currentValue ?? 0,
              true,
              props.userCurrency
            )}
          </Text>
        </Group>
        <Group justify="space-between" align="center">
          <Group gap="1rem">
            <Group gap="0.5rem">
              <DatePickerInput
                {...purchaseDate.getInputProps()}
                placeholder="Purchase Date"
                maw={400}
                clearable
                label={
                  <Text size="xs" fw={600}>
                    Purchase Date
                  </Text>
                }
              />
              <NumberInput
                {...purchasePrice.getInputProps()}
                placeholder="Enter Price"
                maw={150}
                prefix={getCurrencySymbol(props.userCurrency)}
                thousandSeparator=","
                decimalScale={2}
                fixedDecimalScale
                onBlur={() => doUpdateAsset.mutate()}
                label={
                  <Text size="xs" fw={600}>
                    Purchase Price
                  </Text>
                }
              />
            </Group>
            <Group gap="0.5rem">
              <DatePickerInput
                {...sellDate.getInputProps()}
                placeholder="Enter Date"
                maw={400}
                clearable
                label={
                  <Text size="xs" fw={600}>
                    Sell Date
                  </Text>
                }
              />
              <NumberInput
                {...sellPrice.getInputProps()}
                placeholder="Sell Price"
                maw={150}
                prefix={getCurrencySymbol(props.userCurrency)}
                thousandSeparator=","
                decimalScale={2}
                fixedDecimalScale
                onBlur={() => doUpdateAsset.mutate()}
                label={
                  <Text size="xs" fw={600}>
                    Sell Price
                  </Text>
                }
              />
            </Group>
          </Group>
          <Text fw={600} size="sm" c="dimmed">
            Last Updated:{" "}
            {props.asset.valueDate
              ? new Date(props.asset.valueDate).toLocaleDateString()
              : "Never!"}
          </Text>
        </Group>
      </Stack>
      <Group style={{ alignSelf: "stretch" }}>
        {props.asset.deleted ? (
          <ActionIcon
            h="100%"
            size="sm"
            onClick={() => doRestoreAsset.mutate()}
          >
            <Undo2Icon size={16} />
          </ActionIcon>
        ) : (
          <ActionIcon
            h="100%"
            size="sm"
            bg="red"
            onClick={() => doDeleteAsset.mutate()}
          >
            <Trash2Icon size={16} />
          </ActionIcon>
        )}
      </Group>
    </Group>
  );
};

export default EditableAssetItemContent;
