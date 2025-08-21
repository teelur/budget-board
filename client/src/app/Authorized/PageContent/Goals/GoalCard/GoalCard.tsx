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
      onClick={toggle}
      bg={isSelected ? "var(--mantine-primary-color-light)" : ""}
    >
      {isSelected ? (
        <EditableGoalCardContent
          goal={props.goal}
          includeInterest={props.includeInterest}
        />
      ) : (
        <GoalCardContent
          goal={props.goal}
          includeInterest={props.includeInterest}
        />
      )}
    </Card>
  );
};

export default GoalCard;
