import { ActionIcon, Badge, Group, Stack } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import React from "react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAssetResponse } from "~/models/asset";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { useTranslation } from "react-i18next";

interface AssetItemContentProps {
  asset: IAssetResponse;
  userCurrency: string;
  toggle: () => void;
}

const AssetItemContent = (props: AssetItemContentProps): React.ReactNode => {
  const { t } = useTranslation();

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
            props.userCurrency
          )}
        </StatusText>
      </Group>
      <Group justify="space-between" align="center">
        {props.asset.purchaseDate && props.asset.purchasePrice ? (
          <DimmedText size="sm">
            {t("purchased_on_for", {
              date: new Date(props.asset.purchaseDate).toLocaleDateString(),
              price: convertNumberToCurrency(
                props.asset.purchasePrice ?? 0,
                true,
                props.userCurrency
              ),
            })}
          </DimmedText>
        ) : (
          <DimmedText size="sm">{t("no_purchase_info_available")}</DimmedText>
        )}
        <DimmedText size="sm">
          {t("last_updated", {
            date: props.asset.valueDate
              ? new Date(props.asset.valueDate).toLocaleDateString()
              : t("never"),
          })}
        </DimmedText>
      </Group>
    </Stack>
  );
};

export default AssetItemContent;
