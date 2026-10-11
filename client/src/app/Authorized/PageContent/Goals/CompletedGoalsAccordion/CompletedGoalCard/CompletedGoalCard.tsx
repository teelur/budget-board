import { Flex, Group } from "@mantine/core";
import { BodyText, Card } from "@teelur/budget-board-ui";
import React from "react";
import { SignDisplay } from "~/helpers/currency";
import { getGoalTargetAmount } from "~/helpers/goals";
import { IGoalResponse } from "~/models/goal";
import { useSensitiveAmountFormatter } from "~/hooks/useSensitiveAmountFormatter";
import { Trans } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface CompletedGoalCardProps {
  goal: IGoalResponse;
}

const CompletedGoalCard = (props: CompletedGoalCardProps): React.ReactNode => {
  const { dayjs } = useLocale();
  const formatSensitiveAmount = useSensitiveAmountFormatter();

  return (
    <Card>
      <Flex
        justify="space-between"
        align={{ base: "start", xs: "center" }}
        direction={{ base: "column", xs: "row" }}
      >
        <BodyText tone="primary" size="md" fw={500}>
          {props.goal.name}
        </BodyText>
        <Flex
          gap={0}
          direction={{ base: "row", xs: "column" }}
          justify={{ base: "space-between", xs: "flex-end" }}
          w={{ base: "100%", xs: "auto" }}
        >
          <Group gap="0.25rem">
            <Trans
              i18nKey="goal_completed_total_styled"
              values={{
                amount: formatSensitiveAmount(
                  getGoalTargetAmount(
                    props.goal.amount,
                    props.goal.initialAmount,
                  ),
                  SignDisplay.Auto,
                ),
              }}
              components={[
                <BodyText tone="secondary" size="sm" key="label" />,
                <BodyText tone="primary" size="md" key="amount" fw={500} />,
              ]}
            />
          </Group>
          <Group gap="0.25rem">
            <Trans
              i18nKey="goal_completed_date_styled"
              values={{
                date: dayjs(props.goal.completed).format("MMMM YYYY"),
              }}
              components={[
                <BodyText tone="secondary" size="sm" key="label" />,
                <BodyText tone="primary" size="md" key="amount" fw={500} />,
              ]}
            />
          </Group>
        </Flex>
      </Flex>
    </Card>
  );
};

export default CompletedGoalCard;
