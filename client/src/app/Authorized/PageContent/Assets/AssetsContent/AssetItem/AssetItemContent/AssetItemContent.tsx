import { Group, Stack } from "@mantine/core";
import { ActionIcon, Badge, AmountText } from "@teelur/budget-board-ui";
import { ChevronRightIcon, PencilIcon } from "lucide-react";
import React from "react";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { SignDisplay } from "~/helpers/currency";
import SensitiveAmount, {
  useSensitiveAmountFormatter,
} from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import { IAssetResponse } from "~/models/asset";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { getIsParentAssetType, getParentAssetType } from "~/helpers/assets";

interface AssetItemContentProps {
  asset: IAssetResponse;
  toggle: () => void;
}

const AssetItemContent = (props: AssetItemContentProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const formatSensitiveAmount = useSensitiveAmountFormatter();
  const { allAssetTypes } = useAssetTypes();
  const { isPrivacyModeEnabled } = usePrivacyMode();

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
            variant="ghost"
            color="primary"
            size="compact-xs"
            onClick={(e) => {
              e.stopPropagation();
              props.toggle();
            }}
          >
            <PencilIcon size={16} />
          </ActionIcon>
          {props.asset.sellDate && props.asset.sellPrice && (
            <Badge variant="filled" color="success" size="xs">
              {t("sold")}
            </Badge>
          )}
          {props.asset.hide && (
            <Badge variant="filled" color="primary" size="xs">
              {t("hidden")}
            </Badge>
          )}
        </Group>
        <AmountText
          amount={props.asset.currentValue ?? 0}
          size="md"
          isSensitive={isPrivacyModeEnabled}
          locale={intlLocale}
          currency={preferredCurrency}
          decimalPlaces={decimalPlaces}
        />
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
                price: formatSensitiveAmount(
                  props.asset.purchasePrice ?? 0,
                  SignDisplay.Auto,
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
