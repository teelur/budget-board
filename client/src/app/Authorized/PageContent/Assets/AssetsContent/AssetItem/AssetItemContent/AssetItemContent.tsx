import surfaceClasses from "~/styles/Surface.module.css";

import { ActionIcon, Badge, Group, Stack, Text } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import React from "react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAssetResponse } from "~/models/asset";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/Text/DimmedText/DimmedText";
import StatusText from "~/components/Text/StatusText/StatusText";

interface AssetItemContentProps {
  asset: IAssetResponse;
  userCurrency: string;
  toggle: () => void;
}

const AssetItemContent = (props: AssetItemContentProps): React.ReactNode => {
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
            <Badge bg="green">Sold</Badge>
          )}
          {props.asset.hide && <Badge bg="yellow">Hidden</Badge>}
          {props.asset.deleted && <Badge bg="red">Deleted</Badge>}
        </Group>
        <StatusText size="md" value={props.asset.currentValue ?? 0}>
          {convertNumberToCurrency(
            props.asset.currentValue ?? 0,
            true,
            props.userCurrency
          )}
        </StatusText>
      </Group>
      <Group justify="space-between" align="center">
        {props.asset.purchaseDate && props.asset.purchasePrice ? (
          <Text className={surfaceClasses.textDimmed} fw={600} size="sm">
            Purchased on{" "}
            {new Date(props.asset.purchaseDate).toLocaleDateString()} for{" "}
            {convertNumberToCurrency(
              props.asset.purchasePrice ?? 0,
              true,
              props.userCurrency
            )}
            .
          </Text>
        ) : (
          <DimmedText size="sm">No purchase info available.</DimmedText>
        )}
        <DimmedText size="sm">
          Last Updated:{" "}
          {props.asset.valueDate
            ? new Date(props.asset.valueDate).toLocaleDateString()
            : "Never!"}
        </DimmedText>
      </Group>
    </Stack>
  );
};

export default AssetItemContent;
