import {
  ActionIcon,
  Group,
  Stack,
  LoadingOverlay,
  Button,
  Flex,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { PencilIcon, Trash2Icon } from "lucide-react";
import React from "react";
import {
  convertNumberToCurrency,
  getCurrencySymbol,
  SignDisplay,
} from "~/helpers/currency";
import { IAssetResponse, IAssetUpdateRequest } from "~/models/asset";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useTranslation } from "react-i18next";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { useUpdateAssetMutation } from "~/hooks/mutations/assets/useUpdateAssetMutation";
import { useDeleteAssetsMutation } from "~/hooks/mutations/assets/useDeleteAssetsMutation";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface EditableAssetItemContentProps {
  asset: IAssetResponse;
  toggle: () => void;
}

const EditableAssetItemContent = (
  props: EditableAssetItemContentProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    intlLocale,
    dateFormat,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { allAssetTypes } = useAssetTypes();
  const updateAssetMutation = useUpdateAssetMutation();
  const deleteAssetMutation = useDeleteAssetsMutation();

  const assetNameField = useField<string>({
    initialValue: props.asset.name,
  });
  const typeField = useField<string>({
    initialValue: props.asset.type ?? "",
  });
  const purchaseDate = useField<Date | null>({
    initialValue: props.asset.purchaseDate
      ? dayjs(props.asset.purchaseDate).toDate()
      : null,
  });
  const purchasePrice = useField<string | number | undefined>({
    initialValue: props.asset.purchasePrice ?? undefined,
  });
  const sellDate = useField<Date | null>({
    initialValue: props.asset.sellDate
      ? dayjs(props.asset.sellDate).toDate()
      : null,
  });
  const sellPrice = useField<string | number | undefined>({
    initialValue: props.asset.sellPrice ?? undefined,
  });
  const hideAssetField = useField<boolean>({
    initialValue: props.asset.hide,
  });

  return (
    <Group w="100%" gap="0.5rem" wrap="nowrap" align="flex-start">
      <Stack gap="0.5rem" flex="1 1 auto">
        <LoadingOverlay
          visible={
            updateAssetMutation.isPending || deleteAssetMutation.isPending
          }
        />
        <Group justify="space-between" align="flex-end">
          <Group gap="0.5rem" align="flex-end">
            <TextInput
              {...assetNameField.getInputProps()}
              onBlur={() =>
                updateAssetMutation.mutate({
                  id: props.asset.id,
                  name: assetNameField.getValue(),
                } as IAssetUpdateRequest)
              }
              elevation={1}
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
            <Button
              bg={
                hideAssetField.getValue()
                  ? "var(--button-color-warning)"
                  : undefined
              }
              variant={hideAssetField.getValue() ? "filled" : "outline"}
              onClick={() => {
                updateAssetMutation.mutate(
                  {
                    id: props.asset.id,
                    hide: !hideAssetField.getValue(),
                  } as IAssetUpdateRequest,
                  {
                    onSuccess: () => {
                      hideAssetField.setValue(!hideAssetField.getValue());
                    },
                  },
                );
              }}
            >
              {t("hide_asset")}
            </Button>
          </Group>
          <StatusText size="md" amount={props.asset.currentValue}>
            {convertNumberToCurrency(
              props.asset.currentValue ?? 0,
              true,
              preferredCurrency,
              SignDisplay.Auto,
              intlLocale,
            )}
          </StatusText>
        </Group>
        <Group justify="space-between" align="flex-end">
          <Group gap="1rem" align="flex-end">
            <CategorySelect
              w={220}
              categories={allAssetTypes}
              value={typeField.getValue()}
              onChange={(val: string) => {
                updateAssetMutation.mutate({
                  id: props.asset.id,
                  type: val,
                } as IAssetUpdateRequest);
                typeField.setValue(val);
              }}
              withinPortal
              elevation={1}
            />
            <Group gap="0.5rem">
              <DateInput
                {...purchaseDate.getInputProps()}
                locale={dayjsLocale}
                valueFormat={longDateFormat}
                placeholder={t("enter_date")}
                maw={400}
                clearable
                label={
                  <PrimaryText size="xs">{t("purchase_date")}</PrimaryText>
                }
                onChange={(date) => {
                  updateAssetMutation.mutate({
                    id: props.asset.id,
                    purchaseDate: dayjs(date).isValid()
                      ? dayjs(date).format("YYYY-MM-DD")
                      : null,
                  } as IAssetUpdateRequest);
                  purchaseDate.getInputProps().onChange(date);
                }}
                elevation={1}
              />
              <NumberInput
                {...purchasePrice.getInputProps()}
                placeholder={t("enter_price")}
                maw={150}
                prefix={getCurrencySymbol(preferredCurrency)}
                thousandSeparator={thousandsSeparator}
                decimalSeparator={decimalSeparator}
                decimalScale={2}
                fixedDecimalScale
                onBlur={() =>
                  updateAssetMutation.mutate({
                    id: props.asset.id,
                    purchasePrice:
                      purchasePrice.getValue() === undefined ||
                      purchasePrice.getValue() === ""
                        ? null
                        : Number(purchasePrice.getValue()),
                  } as IAssetUpdateRequest)
                }
                label={
                  <PrimaryText size="xs">{t("purchase_price")}</PrimaryText>
                }
                elevation={1}
              />
            </Group>
            <Group gap="0.5rem">
              <DateInput
                {...sellDate.getInputProps()}
                locale={dayjsLocale}
                valueFormat={longDateFormat}
                placeholder={t("enter_date")}
                maw={400}
                clearable
                label={<PrimaryText size="xs">{t("sell_date")}</PrimaryText>}
                onChange={(date) => {
                  updateAssetMutation.mutate({
                    id: props.asset.id,
                    sellDate: dayjs(date).isValid()
                      ? dayjs(date).format("YYYY-MM-DD")
                      : null,
                  } as IAssetUpdateRequest);
                  sellDate.getInputProps().onChange(date);
                }}
                elevation={1}
              />
              <NumberInput
                {...sellPrice.getInputProps()}
                placeholder={t("enter_price")}
                maw={150}
                prefix={getCurrencySymbol(preferredCurrency)}
                thousandSeparator={thousandsSeparator}
                decimalSeparator={decimalSeparator}
                decimalScale={2}
                fixedDecimalScale
                onBlur={() => {
                  updateAssetMutation.mutate({
                    id: props.asset.id,
                    sellPrice:
                      sellPrice.getValue() === undefined ||
                      sellPrice.getValue() === ""
                        ? null
                        : Number(sellPrice.getValue()),
                  } as IAssetUpdateRequest);
                }}
                label={<PrimaryText size="xs">{t("sell_price")}</PrimaryText>}
                elevation={1}
              />
            </Group>
          </Group>
          <DimmedText size="sm">
            {t("last_updated", {
              date: dayjs(props.asset.valueDate).isValid()
                ? dayjs(props.asset.valueDate).format(`${dateFormat}`)
                : t("never"),
            })}
          </DimmedText>
        </Group>
      </Stack>
      <Group style={{ alignSelf: "stretch" }}>
        <ActionIcon
          h="100%"
          size="sm"
          bg="var(--button-color-destructive)"
          onClick={() => deleteAssetMutation.mutate(props.asset.id)}
        >
          <Trash2Icon size={16} />
        </ActionIcon>
      </Group>
    </Group>
  );
};

export default EditableAssetItemContent;
