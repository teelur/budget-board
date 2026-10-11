import cardClasses from "../BudgetsGroup/BudgetParentCard/BudgetParentCard.module.css";

import { Group } from "@mantine/core";
import { HeadingText } from "@teelur/budget-board-ui";
import React from "react";

interface BudgetGroupHeaderProps {
  groupName: string;
}

const BudgetsGroupHeader = (props: BudgetGroupHeaderProps): React.ReactNode => {
  return (
    <Group className={cardClasses.dataContainer} px="0.5rem">
      <Group className={cardClasses.budgetNameContainer}>
        <HeadingText tone="heading" level={4}>
          {props.groupName}
        </HeadingText>
      </Group>
    </Group>
  );
};

export default BudgetsGroupHeader;
