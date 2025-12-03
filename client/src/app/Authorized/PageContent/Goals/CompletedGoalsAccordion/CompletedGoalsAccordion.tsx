import { Accordion as MantineAccordion, Stack } from "@mantine/core";
import React from "react";
import { IGoalResponse } from "~/models/goal";
import CompletedGoalCard from "./CompletedGoalCard/CompletedGoalCard";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Accordion from "~/components/core/Accordion/Accordion";

interface CompletedGoalsAccordionProps {
  compeltedGoals: IGoalResponse[];
}

const CompletedGoalsAccordion = (
  props: CompletedGoalsAccordionProps
): React.ReactNode => {
  return (
    <Accordion elevation={1}>
      <MantineAccordion.Item value="completed-goals">
        <MantineAccordion.Control>
          <PrimaryText>Completed Goals</PrimaryText>
        </MantineAccordion.Control>
        <MantineAccordion.Panel>
          <Stack gap="0.5rem">
            {props.compeltedGoals.map((completedGoal) => (
              <CompletedGoalCard key={completedGoal.id} goal={completedGoal} />
            ))}
          </Stack>
        </MantineAccordion.Panel>
      </MantineAccordion.Item>
    </Accordion>
  );
};

export default CompletedGoalsAccordion;
