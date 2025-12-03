import React from "react";
import { IGoalResponse } from "~/models/goal";
import { useDisclosure } from "@mantine/hooks";
import EditableGoalCardContent from "./EditableGoalCardContent/EditableGoalCardContent";
import GoalCardContent from "./GoalCardContent/GoalCardContent";
import Card from "~/components/core/Card/Card";

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
      onClick={(e: React.MouseEvent) => {
        e.stopPropagation();
        if (!isEditing) {
          props.openGoalDetails(props.goal);
        }
      }}
      hoverEffect={!isEditing}
      elevation={1}
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
