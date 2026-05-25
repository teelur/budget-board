import cardClasses from "../BudgetsGroup/BudgetParentCard/BudgetParentCard.module.css";

import { Group } from "@mantine/core";
import React from "react";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";

interface BudgetGroupHeaderProps {
  groupName: string;
}

const BudgetsGroupHeader = (props: BudgetGroupHeaderProps): React.ReactNode => {
  return (
    <Group className={cardClasses.dataContainer} px="0.5rem">
      <Group className={cardClasses.budgetNameContainer}>
        <PrimaryHeading order={5}>{props.groupName}</PrimaryHeading>
      </Group>
    </Group>
  );
};

export default BudgetsGroupHeader;
