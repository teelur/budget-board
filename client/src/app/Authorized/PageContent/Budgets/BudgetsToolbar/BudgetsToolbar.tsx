import MonthToolcards from "~/components/MonthToolcards/MonthToolcards";
import { initCurrentMonth } from "~/helpers/datetime";
import { Button, Group, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import React from "react";
import AddBudget from "./AddBudget/AddBudget";
import { ICategory } from "~/models/category";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { IBudget, IBudgetCreateRequest } from "~/models/budget";
import { AxiosError, AxiosResponse } from "axios";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";

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

  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doCopyBudget = useMutation({
    mutationFn: async (newBudgets: IBudgetCreateRequest[]) =>
      await request({
        url: "/api/budget",
        method: "POST",
        data: newBudgets,
      }),
    onSuccess: async (_, variables: IBudgetCreateRequest[]) =>
      await queryClient.invalidateQueries({
        queryKey: ["budgets", variables[0]?.date],
      }),
    onError: (error: AxiosError) =>
      notifications.show({ message: translateAxiosError(error), color: "red" }),
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
            color: "red",
          });
        }
      })
      .catch(() => {
        notifications.show({
          message: "There was an error copying the previous month's budget.",
          color: "red",
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
    <Stack>
      <Button
        onClick={toggleSelectMultiple}
        variant="outline"
        color={canSelectMultiple ? "green" : "gray"}
      >
        Select Multiple
      </Button>
      <MonthToolcards
        selectedDates={props.selectedDates}
        setSelectedDates={props.setSelectedDates}
        timeToMonthlyTotalsMap={props.timeToMonthlyTotalsMap}
        isPending={props.isPending}
        allowSelectMultiple={canSelectMultiple}
      />
      <Group justify="flex-end">
        {props.showCopy && (
          <Button onClick={onCopyBudgets} loading={doCopyBudget.isPending}>
            Copy Previous Month
          </Button>
        )}
        {props.selectedDates.length === 1 && (
          <AddBudget
            date={props.selectedDates[0]!}
            categories={props.categories}
          />
        )}
      </Group>
    </Stack>
  );
};

export default BudgetsToolbar;
