import { StatusColorType } from "~/helpers/budgets";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
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
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface BudgetSummaryItemProps {
  label: string;
  amount: number;
  total?: number;
  budgetValueType: StatusColorType;
  hideProgress?: boolean;
  showDivider?: boolean;
}

const BudgetSummaryItem = (props: BudgetSummaryItemProps): React.ReactNode => {
  const { intlLocale } = useLocale();
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
      100,
  );

  const currency = userSettingsQuery.data?.currency ?? "USD";
  const warningThreshold = userSettingsQuery.data?.budgetWarningThreshold ?? 80;
  const signedAmount =
    props.amount * (props.budgetValueType === StatusColorType.Expense ? -1 : 1);

  const formattedAmount = convertNumberToCurrency(
    signedAmount,
    false,
    currency,
    SignDisplay.Auto,
    intlLocale,
  );
  const formattedTotal = convertNumberToCurrency(
    props.total ?? 0,
    false,
    currency,
    SignDisplay.Auto,
    intlLocale,
  );

  const statusTextProps = {
    amount: props.amount,
    total: props.total ?? 0,
    type: props.budgetValueType,
    warningThreshold,
    size: "md" as const,
  };

  const i18nKey = props.total
    ? "budget_amount_fraction_styled"
    : "budget_amount_fraction_no_total_styled";

  const transValues = props.total
    ? { amount: formattedAmount, total: formattedTotal }
    : { amount: formattedAmount };

  const transComponents = props.total
    ? [
        <StatusText {...statusTextProps} key="amount" />,
        <DimmedText size="sm" key="of" />,
        <PrimaryText size="md" key="total" />,
      ]
    : [<StatusText {...statusTextProps} key="amount" />];

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
          <Trans
            i18nKey={i18nKey}
            values={transValues}
            components={transComponents}
          />
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
          warningThreshold={warningThreshold}
          elevation={2}
        />
      )}
    </Stack>
  );
};

export default BudgetSummaryItem;
