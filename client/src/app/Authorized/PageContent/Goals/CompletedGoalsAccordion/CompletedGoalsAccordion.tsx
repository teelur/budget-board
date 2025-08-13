import { Accordion, Stack, Text } from "@mantine/core";
import React from "react";
import { IGoalResponse } from "~/models/goal";
import CompletedGoalCard from "./CompletedGoalCard/CompletedGoalCard";

interface CompletedGoalsAccordionProps {
  compeltedGoals: IGoalResponse[];
}

const CompletedGoalsAccordion = (
  props: CompletedGoalsAccordionProps
): React.ReactNode => {
  return (
    <Accordion variant="separated">
      <Accordion.Item
        value="completed-goals"
        bg="var(--mantine-color-accordion-alternate)"
      >
        <Accordion.Control>
          <Text fw={600}>Completed Goals</Text>
        </Accordion.Control>
        <Accordion.Panel>
          <Stack gap="0.5rem">
            {props.compeltedGoals.map((completedGoal) => (
              <CompletedGoalCard key={completedGoal.id} goal={completedGoal} />
            ))}
          </Stack>
        </Accordion.Panel>
      </Accordion.Item>
    </Accordion>
  );
};

export default CompletedGoalsAccordion;
