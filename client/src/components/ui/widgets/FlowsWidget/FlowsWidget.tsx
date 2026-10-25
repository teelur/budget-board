import { Group } from "@mantine/core";
import { BodyText, Card, HeadingText, Skeleton } from "@teelur/budget-board-ui";
import { GitBranchIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import FlowsChart from "~/components/Charts/FlowsChart/FlowsChart";
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
      <Card w="100%" h="100%">
        <Card.Section>
          <Group gap="0.5rem" align="center" wrap="wrap">
            <Group gap="0.5rem" align="center">
              <GitBranchIcon
                size="1.25rem"
                color="var(--bbui-color-primary)"
                aria-hidden="true"
              />
              <HeadingText level={4}>{t("flows")}</HeadingText>
            </Group>
            <BodyText tone="secondary" size="xs">
              {t("flows_widget_month_range", { range: monthRange })}
            </BodyText>
          </Group>
        </Card.Section>
        <Card.Section>
          {isPending ? (
            <Skeleton height="100%" />
          ) : (
            <FlowsChart
              transactions={transactionsQuery.data ?? []}
              categories={allTransactionCategories}
              height={480}
            />
          )}
        </Card.Section>
      </Card>
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
