import { ChartSeries, getFilteredChartTooltipPayload } from "@mantine/charts";
import { getThemeColor, Group, Stack, useMantineTheme } from "@mantine/core";
import { Square } from "lucide-react";
import React from "react";
import Card from "~/components/core/Card/Card";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { getSeriesLabels } from "~/helpers/get-series-labels";

interface ChartTooltipProps {
  label: string | number | undefined;
  payload: readonly Record<string, any>[] | undefined;
  series: ChartSeries[];
  valueFormatter?: (value: number) => string;
  includeTotal?: boolean;
}

const ChartTooltip = (props: ChartTooltipProps): React.ReactNode => {
  const theme = useMantineTheme();

  const labels = getSeriesLabels(props.series);

  const filteredPayload = getFilteredChartTooltipPayload([...(props.payload ?? [])]);

  if (filteredPayload.length === 0) {
    return null;
  }

  return (
    <Card elevation={0}>
      <Stack gap="1rem">
        <PrimaryText>{props.label}</PrimaryText>
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
                <DimmedText size="sm">
                  {labels[item.name] ?? item.name}
                </DimmedText>
              </Group>
              <PrimaryText size="sm">
                {typeof props.valueFormatter === "function"
                  ? props.valueFormatter(item.value)
                  : item.value}
              </PrimaryText>
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
              <DimmedText size="sm">Total</DimmedText>
            </Group>
            <PrimaryText size="sm">
              {typeof props.valueFormatter === "function"
                ? props.valueFormatter(
                    filteredPayload.reduce((acc, item) => acc + item.value, 0)
                  )
                : filteredPayload.reduce((acc, item) => acc + item.value, 0)}
            </PrimaryText>
          </Group>
        )}
      </Stack>
    </Card>
  );
};

export default ChartTooltip;
