import React from "react";
import { IGoalResponse } from "~/models/goal";
import { useDisclosure } from "@mantine/hooks";
import EditableGoalCardContent from "./EditableGoalCardContent/EditableGoalCardContent";
import GoalCardContent from "./GoalCardContent/GoalCardContent";
import { Card } from "@teelur/budget-board-ui";

interface GoalCardProps {
  goal: IGoalResponse;
  includeInterest: boolean;
  openGoalDetails: (goal: IGoalResponse) => void;
}

const GoalCard = (props: GoalCardProps): React.ReactNode => {
  const [isEditing, { toggle: toggleEdit }] = useDisclosure();

  return (
    <Card
      w="100%"
      hoverable={!isEditing}
      onClick={(e: React.MouseEvent) => {
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
