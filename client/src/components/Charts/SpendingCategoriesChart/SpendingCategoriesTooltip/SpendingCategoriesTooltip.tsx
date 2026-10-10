import { Group, Stack } from "@mantine/core";
import { BodyText, Card } from "@teelur/budget-board-ui";

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
    <Card>
      <Stack gap="0.125rem">
        {showParent && (
          <BodyText tone="secondary" size="xs">
            {parent}
          </BodyText>
        )}
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
          <BodyText tone="primary" size="sm">
            {name}
          </BodyText>
        </Group>
        <BodyText
          tone="primary"
          size="sm"
          fw={500}
        >{`${valueFormatter(value)} (${percent}%)`}</BodyText>
      </Stack>
    </Card>
  );
};

export default SpendingCategoriesTooltip;
