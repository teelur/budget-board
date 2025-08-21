import classes from "./GoalCard.module.css";

import { Card } from "@mantine/core";
import React from "react";
import { IGoalResponse } from "~/models/goal";
import { useDisclosure } from "@mantine/hooks";
import EditableGoalCardContent from "./EditableGoalCardContent/EditableGoalCardContent";
import GoalCardContent from "./GoalCardContent/GoalCardContent";

interface GoalCardProps {
  goal: IGoalResponse;
  includeInterest: boolean;
}

const GoalCard = (props: GoalCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure();

  return (
    <Card
      className={classes.card}
      radius="sm"
      withBorder
      shadow="sm"
      bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
    >
      {isSelected ? (
        <EditableGoalCardContent
          goal={props.goal}
          includeInterest={props.includeInterest}
          toggleIsSelected={toggle}
        />
      ) : (
        <GoalCardContent
          goal={props.goal}
          includeInterest={props.includeInterest}
          toggleIsSelected={toggle}
        />
      )}
    </Card>
  );
};

export default GoalCard;
