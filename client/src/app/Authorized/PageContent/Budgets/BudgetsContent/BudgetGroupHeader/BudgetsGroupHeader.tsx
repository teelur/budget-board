import cardClasses from "../BudgetsGroup/BudgetParentCard/BudgetParentCard.module.css";
import groupClasses from "./BudgetsGroupHeader.module.css";

import { Group, Text } from "@mantine/core";
import React from "react";

interface BudgetGroupHeaderProps {
  groupName: string;
}

const BudgetsGroupHeader = (props: BudgetGroupHeaderProps): React.ReactNode => {
  return (
    <Group className={cardClasses.dataContainer} px="0.5rem">
      <Group className={cardClasses.budgetNameContainer}>
        <Text className={groupClasses.categoryHeader}>{props.groupName}</Text>
      </Group>
    </Group>
  );
};

export default BudgetsGroupHeader;
