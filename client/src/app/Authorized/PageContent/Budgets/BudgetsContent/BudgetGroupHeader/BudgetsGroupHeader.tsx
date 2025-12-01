import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import cardClasses from "../BudgetsGroup/BudgetParentCard/BudgetParentCard.module.css";

import { Group } from "@mantine/core";
import React from "react";

interface BudgetGroupHeaderProps {
  groupName: string;
}

const BudgetsGroupHeader = (props: BudgetGroupHeaderProps): React.ReactNode => {
  return (
    <Group className={cardClasses.dataContainer} px="0.5rem">
      <Group className={cardClasses.budgetNameContainer}>
        <PrimaryText>{props.groupName}</PrimaryText>
      </Group>
    </Group>
  );
};

export default BudgetsGroupHeader;
