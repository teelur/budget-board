import { ChartSeries, getFilteredChartTooltipPayload } from "@mantine/charts";
import {
  Card,
  getThemeColor,
  Group,
  Stack,
  Text,
  useMantineTheme,
} from "@mantine/core";
import { Square } from "lucide-react";
import React from "react";
import { getSeriesLabels } from "~/helpers/get-series-labels";

interface ChartTooltipProps {
  label: string | number | undefined;
  payload: Record<string, any>[] | undefined;
  series: ChartSeries[];
  valueFormatter?: (value: number) => string;
  includeTotal?: boolean;
}

const ChartTooltip = (props: ChartTooltipProps): React.ReactNode => {
  const theme = useMantineTheme();

  const labels = getSeriesLabels(props.series);

  const filteredPayload = getFilteredChartTooltipPayload(props.payload ?? []);

  if (filteredPayload.length === 0) {
    return null;
  }

  return (
    <Card p="0.75rem" withBorder radius="md">
      <Stack gap="1rem">
        <Text fw={600}>{props.label}</Text>
        <Stack gap="0.25rem">
          {filteredPayload.map((item: any) => (
            <Group
              key={item.name}
              gap="2rem"
              wrap="nowrap"
              justify="space-between"
            >
              <Group gap="0.5rem">
                <Square
                  fill={getThemeColor(item.color, theme)}
                  color={getThemeColor(item.color, theme)}
                  width={18}
                  height={18}
                />
                <Text fw={600} size="sm" c="dimmed">
                  {labels[item.name] ?? item.name}
                </Text>
              </Group>
              <Text fw={600} size="sm">
                {typeof props.valueFormatter === "function"
                  ? props.valueFormatter(item.value)
                  : item.value}
              </Text>
            </Group>
          ))}
        </Stack>
        {props.includeTotal && (
          <Group gap="2rem" wrap="nowrap" justify="space-between">
            <Group gap="0.5rem">
              <Square
                fill={getThemeColor("gray", theme)}
                color={getThemeColor("gray", theme)}
                width={18}
                height={18}
              />
              <Text fw={600} size="sm" c="dimmed">
                Total
              </Text>
            </Group>
            <Text fw={600} size="sm">
              {typeof props.valueFormatter === "function"
                ? props.valueFormatter(
                    filteredPayload.reduce((acc, item) => acc + item.value, 0)
                  )
                : filteredPayload.reduce((acc, item) => acc + item.value, 0)}
            </Text>
          </Group>
        )}
      </Stack>
    </Card>
  );
};

export default ChartTooltip;
