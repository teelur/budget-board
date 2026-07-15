import { ActionIcon, Badge, Group, Stack } from "@mantine/core";
import { ChevronRightIcon, PencilIcon } from "lucide-react";
import React from "react";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { IAssetResponse } from "~/models/asset";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { getIsParentAssetType, getParentAssetType } from "~/helpers/assets";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface AssetItemContentProps {
  asset: IAssetResponse;
  toggle: () => void;
}

const AssetItemContent = (props: AssetItemContentProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const { allAssetTypes } = useAssetTypes();

  const getAssetTypeDisplay = (): React.ReactNode => {
    if (!props.asset.type || props.asset.type.length === 0) {
      return <DimmedText size="sm">{t("no_type")}</DimmedText>;
    }

    const isParentAssetType = getIsParentAssetType(
      props.asset.type,
      allAssetTypes,
    );

    const assetType = isParentAssetType
      ? props.asset.type
      : getParentAssetType(props.asset.type, allAssetTypes);

    return (
      <Group gap="0.25rem">
        <DimmedText size="sm">{assetType}</DimmedText>
        {!isParentAssetType && (
          <>
            <ChevronRightIcon size={14} />
            <DimmedText size="sm">{props.asset.type}</DimmedText>
          </>
        )}
      </Group>
    );
  };

  return (
    <Stack gap={0} flex="1 1 auto">
      <Group justify="space-between" align="center">
        <Group gap="0.5rem" align="center">
          <PrimaryText size="md">{props.asset.name}</PrimaryText>
          <ActionIcon
            variant="transparent"
            size="md"
            onClick={(e) => {
              e.stopPropagation();
              props.toggle();
            }}
          >
            <PencilIcon size={16} />
          </ActionIcon>
          {props.asset.sellDate && props.asset.sellPrice && (
            <Badge bg="var(--button-color-confirm)">{t("sold")}</Badge>
          )}
          {props.asset.hide && (
            <Badge bg="var(--button-color-warning)">{t("hidden")}</Badge>
          )}
        </Group>
        <StatusText amount={props.asset.currentValue ?? 0} size="md">
          {convertNumberToCurrency(
            props.asset.currentValue ?? 0,
            true,
            preferredCurrency,
            SignDisplay.Auto,
            intlLocale,
          )}
        </StatusText>
      </Group>
      <Group justify="space-between" align="center">
        <Group gap="0.5rem">
          {getAssetTypeDisplay()}
          <DimmedText size="sm">·</DimmedText>
          {dayjs(props.asset.purchaseDate).isValid() &&
          props.asset.purchasePrice ? (
            <DimmedText size="sm">
              {t("purchased_on_for", {
                date: dayjs(props.asset.purchaseDate).format(dateFormat),
                price: convertNumberToCurrency(
                  props.asset.purchasePrice ?? 0,
                  true,
                  preferredCurrency,
                  SignDisplay.Auto,
                  intlLocale,
                ),
              })}
            </DimmedText>
          ) : (
            <DimmedText size="sm">{t("no_purchase_info_available")}</DimmedText>
          )}
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
  );
};

export default AssetItemContent;
