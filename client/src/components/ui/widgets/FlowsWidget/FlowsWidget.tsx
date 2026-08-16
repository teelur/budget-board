import { Box, Group, Skeleton, Stack } from "@mantine/core";
import { GitBranchIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import FlowsChart from "~/components/Charts/FlowsChart/FlowsChart";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { parseFlowsConfiguration } from "~/helpers/widgets";
import { useTransactionsQuery } from "~/hooks/queries/useTransactionsQuery";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import FlowsWidgetSettings from "./FlowsWidgetSettings/FlowsWidgetSettings";

interface FlowsWidgetProps {
  widget: IWidgetSettingsResponse;
  settingsOpened?: boolean;
  onSettingsClose?: () => void;
}

const FlowsWidget = ({
  widget,
  settingsOpened,
  onSettingsClose,
}: FlowsWidgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const { allTransactionCategories, isPending: categoriesPending } =
    useTransactionCategories();
  const [initializedConfiguration, setInitializedConfiguration] =
    React.useState(widget.configuration);

  const configuration = React.useMemo(
    () => parseFlowsConfiguration(initializedConfiguration),
    [initializedConfiguration],
  );
  const currentMonth = React.useMemo(() => dayjs().startOf("month"), [dayjs]);
  const selectedMonths = React.useMemo(
    () =>
      Array.from({ length: configuration.monthCount }, (_, index) =>
        currentMonth.subtract(index, "month").toDate(),
      ),
    [configuration.monthCount, currentMonth],
  );
  const monthRange = React.useMemo(() => {
    const firstMonth = selectedMonths.at(-1) ?? currentMonth.toDate();
    const start = dayjs(firstMonth).format("MMM YYYY");
    const end = currentMonth.format("MMM YYYY");

    return start === end ? start : `${start} - ${end}`;
  }, [currentMonth, dayjs, selectedMonths]);

  React.useEffect(() => {
    setInitializedConfiguration(widget.configuration);
  }, [widget.configuration]);

  const transactionsQuery = useTransactionsQuery({
    selectedDates: selectedMonths.map((month) => ({
      month: dayjs(month).month() + 1,
      year: dayjs(month).year(),
    })),
  });

  const isPending = transactionsQuery.isPending || categoriesPending;

  return (
    <>
      <SplitCard
        w="100%"
        h="100%"
        border={BorderThickness.Thick}
        header={
          <Group gap="0.5rem" align="baseline" wrap="wrap">
            <GitBranchIcon color="var(--base-color-text-dimmed)" />
            <PrimaryHeading order={3} lh={1}>
              {t("flows")}
            </PrimaryHeading>
            <DimmedText size="xs" lh={1.2}>
              {t("flows_widget_month_range", { range: monthRange })}
            </DimmedText>
          </Group>
        }
        elevation={1}
      >
        <Stack gap={0} w="100%" p="0.5rem" style={{ flex: 1, minHeight: 0 }}>
          {isPending ? (
            <Skeleton height="100%" radius="md" />
          ) : (
            <Box style={{ flex: 1, minHeight: 0, overflow: "auto" }}>
              <FlowsChart
                transactions={transactionsQuery.data ?? []}
                categories={allTransactionCategories}
                height={480}
              />
            </Box>
          )}
        </Stack>
      </SplitCard>
      {settingsOpened !== undefined && onSettingsClose && (
        <FlowsWidgetSettings
          widget={widget}
          opened={settingsOpened}
          onClose={onSettingsClose}
        />
      )}
    </>
  );
};

export default FlowsWidget;
