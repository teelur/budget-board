import { Drawer, Skeleton, Text } from "@mantine/core";
import { IGoalResponse } from "~/models/goal";
import React from "react";

interface GoalDetailsProps {
  goal: IGoalResponse | null;
  isOpen: boolean;
  doClose: () => void;
}

const GoalDetails = (props: GoalDetailsProps): React.ReactNode => {
  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.doClose}
      position="right"
      size="md"
      title={
        <Text size="lg" fw={600}>
          Goal Details
        </Text>
      }
    >
      {props.goal === null ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Text>Test</Text>
      )}
    </Drawer>
  );
};

export default GoalDetails;
