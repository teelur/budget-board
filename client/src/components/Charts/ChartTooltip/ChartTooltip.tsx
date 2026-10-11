import { ChartSeries, getFilteredChartTooltipPayload } from "@mantine/charts";
import { getThemeColor, Group, Stack, useMantineTheme } from "@mantine/core";
import { BodyText, Card, HeadingText } from "@teelur/budget-board-ui";
import { Square } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { getSeriesLabels } from "~/helpers/get-series-labels";

interface ChartTooltipProps {
  label: string | number | undefined;
  payload: readonly Record<string, any>[] | undefined;
  series: ChartSeries[];
  valueFormatter?: (value: number) => string;
  includeTotal?: boolean;
}

const ChartTooltip = (props: ChartTooltipProps): React.ReactNode => {
  const { t } = useTranslation();
  const theme = useMantineTheme();

  const labels = getSeriesLabels(props.series);

  const filteredPayload = getFilteredChartTooltipPayload([
    ...(props.payload ?? []),
  ]);

  if (filteredPayload.length === 0) {
    return null;
  }

  return (
    <Card>
      <Stack gap="0.5rem">
        <HeadingText tone="heading" level={6}>
          {props.label}
        </HeadingText>
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
                <BodyText tone="secondary" size="sm">
                  {labels[item.name] ?? item.name}
                </BodyText>
              </Group>
              <BodyText tone="primary" size="sm" fw={500}>
                {typeof props.valueFormatter === "function"
                  ? props.valueFormatter(item.value)
                  : item.value}
              </BodyText>
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
              <BodyText size="sm">{t("total")}</BodyText>
            </Group>
            <BodyText size="sm">
              {typeof props.valueFormatter === "function"
                ? props.valueFormatter(
                    filteredPayload.reduce((acc, item) => acc + item.value, 0),
                  )
                : filteredPayload.reduce((acc, item) => acc + item.value, 0)}
            </BodyText>
          </Group>
        )}
      </Stack>
    </Card>
  );
};

export default ChartTooltip;
