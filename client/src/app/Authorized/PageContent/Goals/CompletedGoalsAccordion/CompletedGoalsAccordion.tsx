import { Accordion, Stack } from "@mantine/core";
import React from "react";
import { IGoalResponse } from "~/models/goal";
import CompletedGoalCard from "./CompletedGoalCard/CompletedGoalCard";
import SurfaceAccordionRoot from "~/components/Accordion/Surface/SurfaceAccordionRoot/SurfaceAccordionRoot";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";

interface CompletedGoalsAccordionProps {
  compeltedGoals: IGoalResponse[];
}

const CompletedGoalsAccordion = (
  props: CompletedGoalsAccordionProps
): React.ReactNode => {
  return (
    <SurfaceAccordionRoot>
      <Accordion.Item value="completed-goals">
        <Accordion.Control>
          <PrimaryText>Completed Goals</PrimaryText>
        </Accordion.Control>
        <Accordion.Panel>
          <Stack gap="0.5rem">
            {props.compeltedGoals.map((completedGoal) => (
              <CompletedGoalCard key={completedGoal.id} goal={completedGoal} />
            ))}
          </Stack>
        </Accordion.Panel>
      </Accordion.Item>
    </SurfaceAccordionRoot>
  );
};

export default CompletedGoalsAccordion;
