import { StatusColorType } from "~/helpers/budgets";
import { convertNumberToCurrency } from "~/helpers/currency";
import { Divider, Flex, Group, Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import { Trans } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

interface BudgetSummaryItemProps {
  label: string;
  amount: number;
  total?: number;
  budgetValueType: StatusColorType;
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
      (props.budgetValueType === StatusColorType.Expense ? -1 : 1)) /
      (props.total ?? 0)) *
      100
  );

  return (
    <Stack gap={0}>
      <Group
        gap="0.25rem"
        justify={props.showDivider ? "center" : "space-between"}
      >
        <Flex>
          <PrimaryText size="md">{props.label}</PrimaryText>
        </Flex>
        {props.showDivider ? (
          <Divider
            color="var(--elevated-color-border)"
            my="sm"
            variant="dashed"
            flex="1 0 auto"
          />
        ) : null}
        <Flex gap="0.25rem" align="baseline">
          {props.total ? (
            <Trans
              i18nKey="budget_amount_fraction_styled"
              values={{
                amount: convertNumberToCurrency(
                  props.amount *
                    (props.budgetValueType === StatusColorType.Expense
                      ? -1
                      : 1),
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                ),
                total: convertNumberToCurrency(
                  props.total ?? 0,
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                ),
              }}
              components={[
                <StatusText
                  amount={props.amount}
                  total={props.total ?? 0}
                  type={props.budgetValueType}
                  warningThreshold={
                    userSettingsQuery.data?.budgetWarningThreshold ?? 80
                  }
                  size="md"
                  key="amount"
                />,
                <DimmedText size="sm" key="of" />,
                <PrimaryText size="md" key="total" />,
              ]}
            />
          ) : (
            <Trans
              i18nKey="budget_amount_fraction_no_total_styled"
              values={{
                amount: convertNumberToCurrency(
                  props.amount *
                    (props.budgetValueType === StatusColorType.Expense
                      ? -1
                      : 1),
                  false,
                  userSettingsQuery.data?.currency ?? "USD"
                ),
              }}
              components={[
                <StatusText
                  amount={props.amount}
                  total={props.total ?? 0}
                  type={props.budgetValueType}
                  warningThreshold={
                    userSettingsQuery.data?.budgetWarningThreshold ?? 80
                  }
                  size="md"
                  key="amount"
                />,
              ]}
            />
          )}
        </Flex>
      </Group>
      {!props.hideProgress && (props.total ?? 0) > 0 && (
        <Progress
          size={16}
          percentComplete={percentComplete}
          amount={props.amount}
          limit={props.total ?? 0}
          type={
            props.budgetValueType === StatusColorType.Income
              ? ProgressType.Income
              : ProgressType.Expense
          }
          warningThreshold={
            userSettingsQuery.data?.budgetWarningThreshold ?? 80
          }
          elevation={2}
        />
      )}
    </Stack>
  );
};

export default BudgetSummaryItem;
