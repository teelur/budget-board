import { hiddenTransactionCategory, ITransaction } from "~/models/transaction";
import React from "react";
import {
  buildSpendingCategoryChartData,
  buildSpendingSubcategoryChartData,
} from "~/helpers/charts";
import {
  getThemeColor,
  Group,
  Skeleton,
  Stack,
  useMantineTheme,
} from "@mantine/core";
import { ICategory } from "~/models/category";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { Pie, PieChart, ResponsiveContainer, Tooltip } from "recharts";
import { useTranslation } from "react-i18next";
import { uncategorizedTransactionCategory } from "~/models/transaction";
import SpendingCategoriesTooltip from "./SpendingCategoriesTooltip/SpendingCategoriesTooltip";
import { areStringsEqual } from "~/helpers/utils";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface SpendingChartProps {
  transactions: ITransaction[];
  categories: ICategory[];
  isPending?: boolean;
  showSubcategories?: boolean;
}

const LABEL_RADIAN = Math.PI / 180;
const LABEL_MIN_SPACING = 20;
const LABEL_MIN_PERCENT = 0.01;
const SPREAD_PASSES = 8;

interface LabelEntry {
  index: number;
  sx: number;
  sy: number;
  mx: number;
  my: number;
  ex: number;
  ey: number;
  isRight: boolean;
  fill: string;
  name: string;
}

function spreadLabelGroups(labels: LabelEntry[]): Map<number, LabelEntry> {
  const left = labels.filter((l) => !l.isRight).sort((a, b) => a.ey - b.ey);
  const right = labels.filter((l) => l.isRight).sort((a, b) => a.ey - b.ey);

  const spread = (arr: LabelEntry[]) => {
    for (let pass = 0; pass < SPREAD_PASSES; pass++) {
      let changed = false;
      for (let i = 1; i < arr.length; i++) {
        const prev = arr[i - 1]!;
        const curr = arr[i]!;
        const min = prev.ey + LABEL_MIN_SPACING;
        if (curr.ey < min) {
          curr.ey = min;
          changed = true;
        }
      }
      for (let i = arr.length - 2; i >= 0; i--) {
        const curr = arr[i]!;
        const next = arr[i + 1]!;
        const max = next.ey - LABEL_MIN_SPACING;
        if (curr.ey > max) {
          curr.ey = max;
          changed = true;
        }
      }
      if (!changed) break;
    }
  };

  spread(left);
  spread(right);

  const map = new Map<number, LabelEntry>();
  for (const l of [...left, ...right]) {
    map.set(l.index, l);
  }
  return map;
}

const SpendingCategoriesChart = (
  props: SpendingChartProps,
): React.ReactNode => {
  const [chartWidth, setChartWidth] = React.useState(0);

  const isNarrow = chartWidth > 0 && chartWidth < 800;
  const showSubcategories = props.showSubcategories ?? true;

  const { t } = useTranslation();
  const { intlLocale } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const theme = useMantineTheme();

  const { innerChartData, outerChartData } = React.useMemo(() => {
    const translateName = (name: string) =>
      name === uncategorizedTransactionCategory ? t("uncategorized") : name;

    const rawInner = buildSpendingCategoryChartData(
      props.transactions,
      props.categories,
    );
    const rawOuter = buildSpendingSubcategoryChartData(
      props.transactions,
      props.categories,
      rawInner,
    );

    const inner = rawInner.map((entry) => ({
      ...entry,
      name: translateName(entry.name),
      fill: getThemeColor(entry.color, theme),
    }));

    const outer = rawOuter.map((entry) => ({
      ...entry,
      name: translateName(entry.name),
      parent: translateName(entry.parent),
      fill: getThemeColor(entry.color, theme),
    }));

    return { innerChartData: inner, outerChartData: outer };
  }, [props.transactions, props.categories, theme, t]);

  const totalSpending = React.useMemo(
    () =>
      props.transactions
        .filter(
          (tx) =>
            !areStringsEqual(tx.category ?? "", "Income") &&
            !areStringsEqual(tx.category ?? "", hiddenTransactionCategory),
        )
        .reduce((sum, tx) => sum + tx.amount * -1, 0),
    [props.transactions],
  );

  // Pre-compute collision-detected label positions from chart geometry.
  // recharts Pie (startAngle=0, clockWise=true): midAngle = (cumPercent + percent/2) * 360
  const labelPositions = React.useMemo<Map<number, LabelEntry> | null>(() => {
    if (isNarrow || chartWidth <= 0) return null;

    const data = showSubcategories ? outerChartData : innerChartData;
    const total = data.reduce((s, e) => s + e.value, 0);
    if (total === 0) return null;

    const or = 140; // outerRadius of the labeled ring in wide mode
    const cx = chartWidth / 2;
    const cy = 425 / 2;

    let cumPercent = 0;
    const rawLabels: LabelEntry[] = [];

    data.forEach((entry, i) => {
      const percent = entry.value / total;
      const midAngle = (cumPercent + percent / 2) * 360;
      cumPercent += percent;

      if (percent < LABEL_MIN_PERCENT) return;

      const angle = -midAngle * LABEL_RADIAN;
      const cos = Math.cos(angle);
      const sin = Math.sin(angle);
      const sx = cx + or * cos;
      const sy = cy + or * sin;
      const mx = cx + (or + 50) * cos;
      const my = cy + (or + 50) * sin;
      const isRight = cos >= 0;
      const ex = mx + (isRight ? 40 : -40);

      rawLabels.push({
        index: i,
        sx,
        sy,
        mx,
        my,
        ex,
        ey: my,
        isRight,
        fill: entry.fill,
        name: entry.name,
      });
    });

    return rawLabels.length > 0 ? spreadLabelGroups(rawLabels) : null;
  }, [innerChartData, outerChartData, showSubcategories, isNarrow, chartWidth]);

  const renderLabelFromMap = React.useCallback(
    (sectorProps: any) => {
      const pos = labelPositions?.get(sectorProps.index);
      if (!pos) return null;
      return (
        <g>
          <path
            d={`M${pos.sx},${pos.sy}L${pos.mx},${pos.ey}L${pos.ex},${pos.ey}`}
            stroke={pos.fill}
            fill="none"
            strokeWidth={1.5}
          />
          <text
            x={pos.ex + (pos.isRight ? 4 : -4)}
            y={pos.ey}
            textAnchor={pos.isRight ? "start" : "end"}
            dominantBaseline="central"
            fill={pos.fill}
            fontWeight={600}
            fontSize="0.875rem"
          >
            {pos.name}
          </text>
        </g>
      );
    },
    [labelPositions],
  );

  if (props.isPending) {
    return <Skeleton height={isNarrow ? 270 : 425} radius="lg" />;
  }

  if (props.transactions.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="sm">
          {t("select_an_account_to_display_the_chart")}
        </DimmedText>
      </Group>
    );
  }

  const innerRadius = showSubcategories
    ? isNarrow
      ? 70
      : 88
    : isNarrow
      ? 120
      : 140;
  const outerRadius = showSubcategories ? (isNarrow ? 75 : 93) : undefined;

  const formatValue = (value: number) =>
    convertNumberToCurrency(
      value,
      true,
      preferredCurrency,
      SignDisplay.Auto,
      intlLocale,
    );

  return (
    <Stack w="100%" gap="xs">
      <ResponsiveContainer
        width="100%"
        height={isNarrow ? 270 : 425}
        onResize={(w) => setChartWidth(w)}
      >
        <PieChart margin={{ top: 20, right: 20, bottom: 20, left: 20 }}>
          <Pie
            data={innerChartData}
            dataKey="value"
            nameKey="name"
            cx="50%"
            cy="50%"
            innerRadius={0}
            outerRadius={innerRadius}
            isAnimationActive={false}
            label={!showSubcategories ? renderLabelFromMap : false}
            labelLine={false}
          />
          {showSubcategories && (
            <Pie
              data={outerChartData}
              dataKey="value"
              nameKey="name"
              cx="50%"
              cy="50%"
              innerRadius={outerRadius}
              outerRadius={isNarrow ? 120 : 140}
              isAnimationActive={false}
              label={renderLabelFromMap}
              labelLine={false}
            />
          )}
          <Tooltip
            content={
              <SpendingCategoriesTooltip
                valueFormatter={formatValue}
                total={innerChartData.reduce((sum, e) => sum + e.value, 0)}
              />
            }
          />
        </PieChart>
      </ResponsiveContainer>
      {isNarrow && (
        <div
          style={{
            display: "flex",
            flexWrap: "wrap",
            justifyContent: "center",
            gap: "0.25rem 1rem",
            padding: "0.25rem 0",
          }}
        >
          {(showSubcategories ? outerChartData : innerChartData).map(
            (entry) => (
              <div
                key={`${showSubcategories ? "subcategory" : "category"}-${entry.name}`}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: "0.375rem",
                }}
              >
                <div
                  style={{
                    width: 10,
                    height: 10,
                    borderRadius: "50%",
                    backgroundColor: entry.fill,
                    flexShrink: 0,
                  }}
                />
                <span
                  style={{
                    color: "var(--base-color-text-dimmed)",
                    fontWeight: 600,
                    fontSize: "0.875rem",
                  }}
                >
                  {entry.name}
                </span>
              </div>
            ),
          )}
        </div>
      )}
      <DimmedText ta="center" size="sm">
        {t("total_spending")}:{" "}
        <PrimaryText span fw={600}>
          {formatValue(totalSpending)}
        </PrimaryText>
      </DimmedText>
    </Stack>
  );
};

export default SpendingCategoriesChart;
