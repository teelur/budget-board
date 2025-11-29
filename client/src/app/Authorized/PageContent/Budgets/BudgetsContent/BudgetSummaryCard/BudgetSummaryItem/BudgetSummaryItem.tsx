import { BudgetValueType, getBudgetValueColor } from "~/helpers/budgets";
import { convertNumberToCurrency } from "~/helpers/currency";
import { Divider, Flex, Group, Progress, Stack, Text } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface BudgetSummaryItemProps {
  label: string;
  amount: number;
  total?: number;
  budgetValueType: BudgetValueType;
  hideProgress?: boolean;
  showDivider?: boolean;
}

const BudgetSummaryItem = (props: BudgetSummaryItemProps): React.ReactNode => {
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const percentComplete = Math.round(
    ((props.amount *
      (props.budgetValueType === BudgetValueType.Expense ? -1 : 1)) /
      (props.total ?? 0)) *
      100
  );

  return (
    <Stack gap="0.25rem">
      <Group
        gap="0.25rem"
        justify={props.showDivider ? "center" : "space-between"}
      >
        <Flex>
          <Text size="md" fw={600}>
            {props.label}
          </Text>
        </Flex>
        {props.showDivider ? (
          <Divider my="sm" variant="dashed" flex="1 0 auto" />
        ) : null}
        <Flex gap="0.25rem">
          {userSettingsQuery.isPending ? null : (
            <Text
              size="md"
              fw={600}
              c={getBudgetValueColor(
                props.amount,
                props.total ?? 0,
                props.budgetValueType,
                userSettingsQuery.data?.budgetWarningThreshold ?? 80
              )}
            >
              {convertNumberToCurrency(
                props.amount *
                  (props.budgetValueType === BudgetValueType.Expense ? -1 : 1),
                false,
                userSettingsQuery.data?.currency ?? "USD"
              )}
            </Text>
          )}
          {props.total ? (
            <Text size="md" fw={600}>
              of
            </Text>
          ) : null}
          {props.total ? (
            <Text size="md" fw={600}>
              {convertNumberToCurrency(
                props.total,
                false,
                userSettingsQuery.data?.currency ?? "USD"
              )}
            </Text>
          ) : null}
        </Flex>
      </Group>
      {!props.hideProgress && (props.total ?? 0) > 0 && (
        <Progress.Root size={16} radius="xl">
          <Progress.Section
            value={percentComplete > 100 ? 100 : percentComplete}
            color={getBudgetValueColor(
              props.amount,
              props.total ?? 0,
              props.budgetValueType,
              userSettingsQuery.data?.budgetWarningThreshold ?? 80
            )}
          >
            <Progress.Label>{percentComplete.toFixed(0)}%</Progress.Label>
          </Progress.Section>
        </Progress.Root>
      )}
    </Stack>
  );
};

export default BudgetSummaryItem;
