import { Drawer, Text } from "@mantine/core";
import React from "react";

interface BudgetDetailsProps {
  isOpen: boolean;
  close: () => void;
  category: string | null;
  month: Date | null;
}

const BudgetDetails = (props: BudgetDetailsProps): React.ReactNode => {
  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.close}
      position="right"
      size="md"
      title={
        <Text size="lg" fw={600}>
          Budget Details
        </Text>
      }
    >
      <p>{props.category ?? "No Category"}</p>
    </Drawer>
  );
};

export default BudgetDetails;
