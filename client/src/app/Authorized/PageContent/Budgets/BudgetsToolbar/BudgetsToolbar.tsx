import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { initCurrentMonth } from "~/helpers/datetime";
import { Button, Group, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import React from "react";
import AddBudget from "./AddBudget/AddBudget";
import { ICategory } from "~/models/category";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { IBudget, IBudgetCreateRequest } from "~/models/budget";
import { AxiosError, AxiosResponse } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import BudgetSettings from "./BudgetSettings/BudgetSettings";

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

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doCopyBudget = useMutation({
    mutationFn: async (newBudgets: IBudgetCreateRequest[]) =>
      await request({
        url: "/api/budget",
        method: "POST",
        data: newBudgets,
        params: { isCopy: true },
      }),
    onSuccess: async (_, variables: IBudgetCreateRequest[]) =>
      await queryClient.invalidateQueries({
        queryKey: ["budgets", variables[0]?.date],
      }),
    onError: (error: AxiosError) =>
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      }),
  });

  const onCopyBudgets = (): void => {
    const lastMonth = new Date(props.selectedDates[0]!);
    lastMonth.setMonth(lastMonth.getMonth() - 1);

    request({
      url: "/api/budget",
      method: "GET",
      params: { date: lastMonth },
    })
      .then((res: AxiosResponse<any, any>) => {
        const budgets: IBudget[] = res.data;
        if (budgets.length !== 0) {
          const newBudgets: IBudgetCreateRequest[] = budgets.map((budget) => {
            return {
              date: props.selectedDates[0],
              category: budget.category,
              limit: budget.limit,
            } as IBudgetCreateRequest;
          });
          doCopyBudget.mutate(newBudgets);
        } else {
          notifications.show({
            message: "Previous month has no budget!",
            color: "var(--button-color-destructive)",
          });
        }
      })
      .catch(() => {
        notifications.show({
          message: "There was an error copying the previous month's budget.",
          color: "var(--button-color-destructive)",
        });
      });
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
          Select Multiple
        </Button>
        <Group gap="0.5rem">
          {props.showCopy && (
            <Button onClick={onCopyBudgets} loading={doCopyBudget.isPending}>
              Copy Previous
            </Button>
          )}
          {props.selectedDates.length === 1 && (
            <AddBudget
              date={props.selectedDates[0]!}
              categories={props.categories}
            />
          )}
          <BudgetSettings />
        </Group>
      </Group>
    </Stack>
  );
};

export default BudgetsToolbar;
