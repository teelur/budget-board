import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { initCurrentMonth } from "~/helpers/datetime";
import { Button, Group, Stack, ActionIcon } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import React from "react";
import AddBudget from "./AddBudget/AddBudget";
import { ICategory } from "~/models/category";
import { IBudgetCreateRequest } from "~/models/budget";
import { notifications } from "@mantine/notifications";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router";
import { SettingsIcon } from "lucide-react";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useCreateBudgetMutation } from "~/hooks/mutations/budgets/useCreateBudgetMutation";
import { useBudgetsQuery } from "~/hooks/queries/useBudgetsQuery";

interface BudgetsToolbarProps {
  categories: ICategory[];
  selectedDates: Date[];
  setSelectedDates: React.Dispatch<React.SetStateAction<Date[]>>;
  timeToMonthlyTotalsMap: Map<number, number>;
  showCopy: boolean;
  isPending: boolean;
}

const BudgetsToolbar = (props: BudgetsToolbarProps): React.ReactNode => {
  const [canSelectMultiple, { toggle }] = useDisclosure(false);

  const { t } = useTranslation();
  const navigate = useNavigate();
  const { dayjs } = useLocale();
  const createBudgetMutation = useCreateBudgetMutation({ isCopying: true });

  const previousMonthDate = React.useMemo(() => {
    if (props.selectedDates.length !== 1) {
      return null;
    }

    const date = new Date(props.selectedDates[0]!);
    date.setMonth(date.getMonth() - 1);
    return date;
  }, [props.selectedDates]);

  const previousMonthBudgetsQuery = useBudgetsQuery({
    months: previousMonthDate ? [previousMonthDate] : [],
    enabled: props.showCopy && previousMonthDate !== null,
  });

  const onCopyBudgets = (): void => {
    const budgets = previousMonthBudgetsQuery.data;

    if (!budgets || budgets.length === 0) {
      notifications.show({
        message: t("budget_previous_month_no_budgets"),
        color: "var(--button-color-destructive)",
      });
      return;
    }

    const newBudgets: IBudgetCreateRequest[] = budgets.map((budget) => {
      return {
        month: dayjs(props.selectedDates[0]!).format("YYYY-MM-DD"),
        category: budget.category,
        limit: budget.limit,
      } as IBudgetCreateRequest;
    });

    createBudgetMutation.mutate(newBudgets);
  };

  const toggleSelectMultiple = () => {
    if (canSelectMultiple) {
      // Need to pick the date used for our single date.
      if (props.selectedDates.length === 0) {
        // When nothing is selected, revert back to today.
        props.setSelectedDates([initCurrentMonth()]);
      } else {
        // Otherwise select the most recent selected date.
        props.setSelectedDates([
          new Date(Math.max(...props.selectedDates.map((d) => d.getTime()))),
        ]);
      }
    }

    toggle();
  };

  return (
    <Stack gap="1rem">
      <MonthToolcards
        selectedDates={props.selectedDates}
        setSelectedDates={props.setSelectedDates}
        timeToMonthlyTotalsMap={props.timeToMonthlyTotalsMap}
        isPending={props.isPending}
        allowSelectMultiple={canSelectMultiple}
        allowFutureMonths
      />
      <Group justify="space-between" gap="0.5rem">
        <Button
          onClick={toggleSelectMultiple}
          variant="outline"
          color={canSelectMultiple ? "var(--button-color-confirm)" : ""}
        >
          {t("select_multiple")}
        </Button>
        <Group gap="0.5rem">
          {props.showCopy && (
            <Button
              onClick={onCopyBudgets}
              loading={
                createBudgetMutation.isPending ||
                previousMonthBudgetsQuery.isPending
              }
            >
              {t("copy_previous")}
            </Button>
          )}
          {props.selectedDates.length === 1 && (
            <AddBudget
              date={props.selectedDates[0]!}
              categories={props.categories}
            />
          )}
          <ActionIcon
            variant="subtle"
            size="input-sm"
            onClick={() => navigate("/budgets/settings")}
          >
            <SettingsIcon />
          </ActionIcon>
        </Group>
      </Group>
    </Stack>
  );
};

export default BudgetsToolbar;
