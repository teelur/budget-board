import { Stack } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import type { FlowsChartData, FlowType } from "~/helpers/charts";
import classes from "./FlowsChartTooltip.module.css";

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

const getFlowClass = (flowType: FlowType | undefined) => {
  switch (flowType) {
    case "income":
      return classes.income;
    case "expense":
      return classes.expense;
    case "surplus":
      return classes.surplus;
    case "deficit":
      return classes.deficit;
    default:
      return "";
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
      ? [{ label: labels.percentOfBranch, value: formatPercent(value, parentValue) }]
      : []),
  ];
  const hasTransactionCount =
    metadata?.transactionCount !== undefined && metadata.transactionCount > 0;
  const tooltipClassName = `${classes.tooltip} ${getFlowClass(flowType)}`;

  return (
    <Card className={tooltipClassName} elevation={0} p="0.75rem">
      <Stack gap="0.75rem">
        {flowLabel && (
          <div className={classes.header}>
            <DimmedText className={classes.flowLabel}>{flowLabel}</DimmedText>
          </div>
        )}
        {isLink ? (
          <Stack className={classes.route} gap="0.25rem">
            <DimmedText className={classes.routeLabel}>{labels.from}</DimmedText>
            <PrimaryText className={classes.routeValue}>{sourceName}</PrimaryText>
            <DimmedText className={classes.routeLabel}>{labels.to}</DimmedText>
            <PrimaryText className={classes.routeValue}>{targetName}</PrimaryText>
          </Stack>
        ) : (
          <PrimaryText className={classes.nodeName}>{item.name}</PrimaryText>
        )}
        <div className={classes.amountBlock}>
          <DimmedText className={classes.amountLabel}>{labels.amount}</DimmedText>
          <PrimaryText className={classes.amount}>{valueFormatter(value)}</PrimaryText>
        </div>
        {(percentageMetrics.length > 0 || hasTransactionCount) && (
          <div className={classes.metrics}>
            {percentageMetrics.map((metric) => {
              const numericPercent = Number.parseFloat(metric.value);
              return (
                <div className={classes.metric} key={metric.label}>
                  <DimmedText className={classes.metricLabel}>{metric.label}</DimmedText>
                  <PrimaryText className={classes.metricValue}>{metric.value}</PrimaryText>
                  <div className={classes.progressTrack}>
                    <div
                      className={classes.progressBar}
                      style={{ width: `${Math.min(numericPercent, 100)}%` }}
                    />
                  </div>
                </div>
              );
            })}
            {hasTransactionCount && (
              <div className={`${classes.metric} ${classes.transactionCount}`}>
                <DimmedText className={classes.metricLabel}>{labels.transactions}</DimmedText>
                <PrimaryText className={classes.metricValue}>
                  {metadata.transactionCount}
                </PrimaryText>
              </div>
            )}
          </div>
        )}
      </Stack>
    </Card>
  );
};

export default FlowsChartTooltip;
