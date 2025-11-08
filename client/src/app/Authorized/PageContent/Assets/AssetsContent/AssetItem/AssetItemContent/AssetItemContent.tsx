import { ActionIcon, Badge, Group, Stack, Text } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import React from "react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAssetResponse } from "~/models/asset";

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
          <Text fw={600} size="md">
            {props.asset.name}
          </Text>
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
        {props.asset.purchaseDate && props.asset.purchasePrice ? (
          <Text fw={600} size="sm" c="dimmed">
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
          <Text fw={600} size="sm" c="dimmed">
            No purchase info available.
          </Text>
        )}
        <Text fw={600} size="sm" c="dimmed">
          Last Updated:{" "}
          {props.asset.valueDate
            ? new Date(props.asset.valueDate).toLocaleDateString()
            : "Never!"}
        </Text>
      </Group>
    </Stack>
  );
};

export default AssetItemContent;
