import { Stack } from "@mantine/core";
import { BodyText } from "@teelur/budget-board-ui";
import React from "react";
import { IGoalResponse } from "~/models/goal";
import CompletedGoalCard from "./CompletedGoalCard/CompletedGoalCard";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";

interface CompletedGoalsAccordionProps {
  compeltedGoals: IGoalResponse[];
}

const CompletedGoalsAccordion = (
  props: CompletedGoalsAccordionProps,
): React.ReactNode => {
  const { t } = useTranslation();
  return (
    <Accordion elevation={1}>
      <Accordion.Item
        title={
          <BodyText tone="heading" size="md" fw={500}>
            {t("completed_goals")}
          </BodyText>
        }
      >
        <Stack gap="0.5rem">
          {props.compeltedGoals.map((completedGoal) => (
            <CompletedGoalCard key={completedGoal.id} goal={completedGoal} />
          ))}
        </Stack>
      </Accordion.Item>
    </Accordion>
  );
};

export default CompletedGoalsAccordion;
