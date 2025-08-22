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
  openGoalDetails: (goal: IGoalResponse) => void;
}

const GoalCard = (props: GoalCardProps): React.ReactNode => {
  const [isEditing, { toggle: toggleEdit }] = useDisclosure();

  return (
    <Card
      className={classes.card}
      radius="sm"
      withBorder
      shadow="sm"
      bg={isEditing ? "var(--mantine-primary-color-light)" : ""}
      onClick={(e) => {
        e.stopPropagation();
        if (!isEditing) {
          props.openGoalDetails(props.goal);
        }
      }}
    >
      {isEditing ? (
        <EditableGoalCardContent
          goal={props.goal}
          includeInterest={props.includeInterest}
          toggleIsSelected={toggleEdit}
        />
      ) : (
        <GoalCardContent
          goal={props.goal}
          includeInterest={props.includeInterest}
          toggleIsSelected={toggleEdit}
        />
      )}
    </Card>
  );
};

export default GoalCard;
