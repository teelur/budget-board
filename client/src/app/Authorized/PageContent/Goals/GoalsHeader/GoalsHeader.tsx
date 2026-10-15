import { Group } from "@mantine/core";
import { Button, SegmentedControl } from "@teelur/budget-board-ui";
import React from "react";
import AddGoalModal from "./AddGoalModal/AddGoalModal";
import { useTranslation } from "react-i18next";
import { GoalCategory } from "../Goals";

interface GoalsHeaderProps {
  includeInterest: boolean;
  toggleIncludeInterest: () => void;
  goalCategory: string;
  setGoalCategory: React.Dispatch<React.SetStateAction<string>>;
}

const GoalsHeader = (props: GoalsHeaderProps): React.ReactNode => {
  const { t } = useTranslation();
  return (
    <Group justify="space-between" align="center">
      <SegmentedControl
        size="xs"
        value={props.goalCategory}
        onChange={(value) => props.setGoalCategory(value)}
        data={[
          {
            label: t("in_progress"),
            value: GoalCategory.InProgress.toString(),
          },
          { label: t("completed"), value: GoalCategory.Completed.toString() },
        ]}
        aria-label={t("goal_category")}
      />
      <Group gap="0.5rem">
        <Button
          variant="filled"
          color="primary"
          size="xs"
          selected={props.includeInterest}
          onClick={props.toggleIncludeInterest}
        >
          {t("include_interest")}
        </Button>
        <AddGoalModal />
      </Group>
    </Group>
  );
};

export default GoalsHeader;
