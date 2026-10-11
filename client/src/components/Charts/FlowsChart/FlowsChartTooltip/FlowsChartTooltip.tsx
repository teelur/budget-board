import { Group, Stack } from "@mantine/core";
import type { FlowsChartData, FlowType } from "~/helpers/charts";
import classes from "./FlowsChartTooltip.module.css";
import { BodyText, Card, HeadingText, Progress } from "@teelur/budget-board-ui";

interface FlowsChartTooltipProps {
  active?: boolean;
  payload?: readonly any[];
  chartData: FlowsChartData;
  valueFormatter: (value: number) => string;
  labels: {
    amount: string;
    from: string;
    to: string;
    percentOfIncome: string;
    percentOfSpending: string;
    percentOfBranch: string;
    transactions: string;
    income: string;
    expense: string;
    surplus: string;
    deficit: string;
  };
}

const formatPercent = (value: number, total: number) =>
  total > 0 ? `${((value / total) * 100).toFixed(1)}%` : "0.0%";

const getFlowLabel = (
  flowType: FlowType | undefined,
  labels: FlowsChartTooltipProps["labels"],
) => {
  switch (flowType) {
    case "income":
      return labels.income;
    case "expense":
      return labels.expense;
    case "surplus":
      return labels.surplus;
    case "deficit":
      return labels.deficit;
    default:
      return null;
  }
};

const FlowsChartTooltip = ({
  active,
  payload,
  chartData,
  valueFormatter,
  labels,
}: FlowsChartTooltipProps): React.ReactNode => {
  if (!active || !payload?.length) {
    return null;
  }

  const item = payload[0];
  const rawPayload = item.payload?.payload ?? item.payload;
  const source = rawPayload?.source;
  const target = rawPayload?.target;
  const isLink = source !== undefined && target !== undefined;
  const value = Number(item.value ?? rawPayload?.value ?? 0);

  const getNodeName = (node: unknown) =>
    typeof node === "object" && node !== null && "name" in node
      ? String(node.name)
      : typeof node === "number"
        ? (chartData.nodes[node]?.name ?? "")
        : String(node ?? "");

  const sourceName = getNodeName(source);
  const targetName = getNodeName(target);
  const matchingLink = isLink
    ? chartData.links.find((link) => {
        const linkSource = chartData.nodes[link.source]?.name;
        const linkTarget = chartData.nodes[link.target]?.name;
        return (
          linkSource === sourceName &&
          linkTarget === targetName &&
          link.value === value
        );
      })
    : undefined;
  const node = !isLink
    ? chartData.nodes.find((entry) => entry.name === item.name)
    : undefined;
  const metadata = isLink ? matchingLink : node;
  const flowType = metadata?.flowType;
  const parentName = node?.parentName;
  const parentIndex = parentName
    ? chartData.nodes.findIndex((entry) => entry.name === parentName)
    : -1;
  const parentValue =
    parentIndex >= 0
      ? Math.max(
          chartData.links
            .filter((link) => link.source === parentIndex)
            .reduce((total, link) => total + link.value, 0),
          chartData.links
            .filter((link) => link.target === parentIndex)
            .reduce((total, link) => total + link.value, 0),
        )
      : 0;
  const flowLabel = getFlowLabel(flowType, labels);
  const percentageMetrics = [
    ...(flowType === "income"
      ? [
          {
            label: labels.percentOfIncome,
            value: formatPercent(value, chartData.totalIncome),
          },
        ]
      : []),
    ...(flowType === "expense"
      ? [
          {
            label: labels.percentOfSpending,
            value: formatPercent(value, chartData.totalSpending),
          },
        ]
      : []),
    ...(parentValue > 0
      ? [
          {
            label: labels.percentOfBranch,
            value: formatPercent(value, parentValue),
          },
        ]
      : []),
  ];
  const hasTransactionCount =
    metadata?.transactionCount !== undefined && metadata.transactionCount > 0;

  return (
    <Card w="min(18rem, calc(100vw - 2rem))">
      <Card.Section>
        <Stack pl="0.625rem" p="0.25rem" gap="0.5rem">
          {flowLabel && (
            <HeadingText tone="heading" level={6}>
              {flowLabel}
            </HeadingText>
          )}
          {isLink ? (
            <Stack className={classes.route} gap="0.25rem">
              <BodyText
                tone="secondary"
                size="sm"
                className={classes.routeLabel}
              >
                {labels.from}
              </BodyText>
              <BodyText tone="primary" fw={500} className={classes.routeValue}>
                {sourceName}
              </BodyText>
              <BodyText tone="secondary" className={classes.routeLabel}>
                {labels.to}
              </BodyText>
              <BodyText tone="primary" fw={500} className={classes.routeValue}>
                {targetName}
              </BodyText>
            </Stack>
          ) : (
            <BodyText tone="primary" fw={500} className={classes.nodeName}>
              {item.name}
            </BodyText>
          )}
        </Stack>
      </Card.Section>
      <Card.Section>
        <Stack pl="0.625rem" p="0.25rem" gap="0.125rem">
          <BodyText tone="secondary" className={classes.amountLabel}>
            {labels.amount}
          </BodyText>
          <BodyText tone="primary" fw={500} className={classes.amount}>
            {valueFormatter(value)}
          </BodyText>
        </Stack>
      </Card.Section>
      <Card.Section>
        <Group pl="0.625rem" pb="0.5rem" p="0.25rem">
          {(percentageMetrics.length > 0 || hasTransactionCount) &&
            percentageMetrics.map((metric) => {
              return (
                <Stack gap="0.125rem" key={metric.label}>
                  <BodyText tone="secondary" className={classes.metricLabel}>
                    {metric.label}
                  </BodyText>
                  <BodyText tone="primary" className={classes.metricValue}>
                    {metric.value}
                  </BodyText>
                  <Progress
                    value={Number.parseFloat(metric.value)}
                    size="xs"
                    ariaLabel={metric.label}
                  />
                </Stack>
              );
            })}
          {hasTransactionCount && (
            <Stack gap="0.125rem">
              <BodyText tone="secondary" className={classes.metricLabel}>
                {labels.transactions}
              </BodyText>
              <BodyText tone="primary" className={classes.metricValue}>
                {metadata.transactionCount}
              </BodyText>
            </Stack>
          )}
        </Group>
      </Card.Section>
    </Card>
  );
};

export default FlowsChartTooltip;
