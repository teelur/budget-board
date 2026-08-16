import { SankeyChart as MantineSankeyChart } from "@mantine/charts";
import { Group, Skeleton } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { buildFlowsChartData, chartColors } from "~/helpers/charts";
import { useSensitiveAmountFormatter } from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import { SignDisplay } from "~/helpers/currency";
import { ICategory } from "~/models/category";
import { ITransaction } from "~/models/transaction";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import type { SankeyElementType, SankeyLinkProps, SankeyProps } from "recharts";
import FlowsChartTooltip from "./FlowsChartTooltip/FlowsChartTooltip";

interface FlowsChartProps {
  transactions: ITransaction[];
  categories: ICategory[];
  isPending?: boolean;
  height?: number;
}

const FlowsChart = (props: FlowsChartProps): React.ReactNode => {
  const { t } = useTranslation();
  const formatSensitiveAmount = useSensitiveAmountFormatter();
  const [hoveredNodeIndex, setHoveredNodeIndex] = React.useState<number | null>(
    null,
  );

  const chartData = React.useMemo(
    () =>
      buildFlowsChartData(props.transactions, props.categories, {
        cashAvailable: t("cash_available"),
        surplus: t("surplus"),
        deficit: t("deficit"),
        total: t("total"),
        income: t("income"),
        expense: t("expense"),
        uncategorized: t("uncategorized"),
      }),
    [props.transactions, props.categories, t],
  );

  const valueFormatter = React.useCallback(
    (value: number) => formatSensitiveAmount(value, false, SignDisplay.Auto),
    [formatSensitiveAmount],
  );

  const chartHeight = props.height ?? 750;

  const highlightedLinkIndexes = React.useMemo(() => {
    if (hoveredNodeIndex === null) {
      return null;
    }

    const cashNodeIndex = chartData.nodes.findIndex(
      (node) => node.name === t("cash_available"),
    );
    if (cashNodeIndex === -1 || hoveredNodeIndex === cashNodeIndex) {
      return null;
    }

    const adjacentLinks = new Map<
      number,
      { linkIndex: number; nodeIndex: number }[]
    >();
    chartData.links.forEach((link, linkIndex) => {
      adjacentLinks.set(link.source, [
        ...(adjacentLinks.get(link.source) ?? []),
        { linkIndex, nodeIndex: link.target },
      ]);
      adjacentLinks.set(link.target, [
        ...(adjacentLinks.get(link.target) ?? []),
        { linkIndex, nodeIndex: link.source },
      ]);
    });

    const queue = [hoveredNodeIndex];
    const previous = new Map<
      number,
      { nodeIndex: number; linkIndex: number }
    >();
    const visited = new Set([hoveredNodeIndex]);

    while (queue.length > 0) {
      const currentNodeIndex = queue.shift()!;
      if (currentNodeIndex === cashNodeIndex) {
        break;
      }

      adjacentLinks
        .get(currentNodeIndex)
        ?.forEach(({ linkIndex, nodeIndex }) => {
          if (!visited.has(nodeIndex)) {
            visited.add(nodeIndex);
            previous.set(nodeIndex, { nodeIndex: currentNodeIndex, linkIndex });
            queue.push(nodeIndex);
          }
        });
    }

    const result = new Set<number>();
    let currentNodeIndex = cashNodeIndex;
    while (previous.has(currentNodeIndex)) {
      const step = previous.get(currentNodeIndex)!;
      result.add(step.linkIndex);
      currentNodeIndex = step.nodeIndex;
    }
    return result;
  }, [chartData, hoveredNodeIndex, t]);

  const handleMouseEnter = React.useCallback<
    NonNullable<SankeyProps["onMouseEnter"]>
  >((item, type: SankeyElementType) => {
    setHoveredNodeIndex(type === "node" ? item.index : null);
  }, []);

  const handleMouseLeave = React.useCallback(() => {
    setHoveredNodeIndex(null);
  }, []);

  const renderLink = React.useCallback(
    (linkProps: SankeyLinkProps) => {
      const isHighlighted =
        highlightedLinkIndexes === null ||
        highlightedLinkIndexes.has(linkProps.index);
      const fill = "var(--chart-link-color)";

      return (
        <path
          d={`
            M${linkProps.sourceX},${linkProps.sourceY + linkProps.linkWidth / 2}
            C${linkProps.sourceControlX},${linkProps.sourceY + linkProps.linkWidth / 2}
              ${linkProps.targetControlX},${linkProps.targetY + linkProps.linkWidth / 2}
              ${linkProps.targetX},${linkProps.targetY + linkProps.linkWidth / 2}
            L${linkProps.targetX},${linkProps.targetY - linkProps.linkWidth / 2}
            C${linkProps.targetControlX},${linkProps.targetY - linkProps.linkWidth / 2}
              ${linkProps.sourceControlX},${linkProps.sourceY - linkProps.linkWidth / 2}
              ${linkProps.sourceX},${linkProps.sourceY - linkProps.linkWidth / 2}
            Z
          `}
          fill={fill}
          opacity={isHighlighted ? 0.65 : 0.08}
          stroke="none"
        />
      );
    },
    [highlightedLinkIndexes],
  );

  if (props.isPending) {
    return <Skeleton height={chartHeight} radius="lg" />;
  }

  if (chartData.links.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="sm">{t("no_flow_data")}</DimmedText>
      </Group>
    );
  }

  return (
    <MantineSankeyChart
      data={chartData}
      height={chartHeight}
      nodePadding={12}
      sankeyProps={{
        link: renderLink,
        margin: { top: 12, right: 8, bottom: 32, left: 8 },
        onMouseEnter: handleMouseEnter,
        onMouseLeave: handleMouseLeave,
        sort: false,
      }}
      tooltipProps={{
        content: ({ active, payload }) => (
          <FlowsChartTooltip
            active={active}
            payload={payload}
            chartData={chartData}
            valueFormatter={valueFormatter}
            labels={{
              amount: t("amount"),
              from: t("from"),
              to: t("to"),
              percentOfIncome: t("percent_of_income"),
              percentOfSpending: t("percent_of_spending"),
              percentOfBranch: t("percent_of_branch"),
              transactions: t("transactions"),
              income: t("income"),
              expense: t("expense"),
              surplus: t("surplus"),
              deficit: t("deficit"),
            }}
          />
        ),
      }}
      w="100%"
      colors={chartColors}
      valueFormatter={valueFormatter}
    />
  );
};

export default FlowsChart;
