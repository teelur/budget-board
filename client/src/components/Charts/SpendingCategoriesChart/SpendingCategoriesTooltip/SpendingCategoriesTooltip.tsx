import { Group, Stack } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface SpendingCategoriesTooltipProps {
  active?: boolean;
  payload?: any[];
  valueFormatter: (value: number) => string;
  total: number;
}

const SpendingCategoriesTooltip = ({
  active,
  payload,
  valueFormatter,
  total,
}: SpendingCategoriesTooltipProps) => {
  if (!active || !payload?.length) return null;
  const { name, value, fill, parent } = payload[0].payload;
  const percent = total > 0 ? ((value / total) * 100).toFixed(1) : "0.0";
  const showParent = parent !== undefined && parent !== name;
  return (
    <Card elevation={0}>
      <Stack gap="0.25rem">
        {showParent && <DimmedText size="xs">{parent}</DimmedText>}
        <Group gap="0.5rem" wrap="nowrap">
          <div
            style={{
              width: 12,
              height: 12,
              borderRadius: 2,
              background: fill,
              flexShrink: 0,
            }}
          />
          <DimmedText size="sm">{name}</DimmedText>
        </Group>
        <PrimaryText size="sm">{`${valueFormatter(value)} (${percent}%)`}</PrimaryText>
      </Stack>
    </Card>
  );
};

export default SpendingCategoriesTooltip;
