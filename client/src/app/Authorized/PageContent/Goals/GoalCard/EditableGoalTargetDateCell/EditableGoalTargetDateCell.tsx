import classes from "./EditableGoalTargetDateCell.module.css";

import { Flex, Text } from "@mantine/core";
import { DatePickerInput } from "@mantine/dates";
import { IGoalResponse, IGoalUpdateRequest } from "~/models/goal";
import React from "react";
import dayjs from "dayjs";

interface EditableGoalTargetDateCellProps {
  goal: IGoalResponse;
  isSelected: boolean;
  editCell: (newGoal: IGoalUpdateRequest) => void;
}

const EditableGoalTargetDateCell = (
  props: EditableGoalTargetDateCellProps
): React.ReactNode => {
  const [goalTargetDateValue, setGoalTargetDateValue] = React.useState<Date>(
    new Date(props.goal.completeDate)
  );

  const onDatePick = (date: string | null): void => {
    if (date === null) {
      return;
    }
    const parsedDate = dayjs(date);
    if (!parsedDate.isValid()) {
      return;
    }
    setGoalTargetDateValue(parsedDate.toDate());
    const newGoal: IGoalUpdateRequest = {
      ...props.goal,
      completeDate: parsedDate.toDate(),
    };
    if (props.editCell != null) {
      props.editCell(newGoal);
    }
  };

  return (
    <Flex className={classes.container}>
      <Text size="sm" fw={600} c="dimmed">
        Projected:{" "}
      </Text>
      {props.isSelected && props.goal.isCompleteDateEditable ? (
        <Flex
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          <DatePickerInput
            className="h-8"
            onChange={onDatePick}
            value={goalTargetDateValue}
          />
        </Flex>
      ) : (
        <Text size="sm" fw={600} c="dimmed">
          {new Date(props.goal.completeDate).toLocaleDateString("en-US", {
            year: "numeric",
            month: "long",
          })}
        </Text>
      )}
    </Flex>
  );
};

export default EditableGoalTargetDateCell;
