import { Group } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import React from "react";
import AddGoalModal from "./AddGoalModal/AddGoalModal";
import { useTranslation } from "react-i18next";

interface GoalsHeaderProps {
  includeInterest: boolean;
  toggleIncludeInterest: () => void;
}

const GoalsHeader = (props: GoalsHeaderProps): React.ReactNode => {
  const { t } = useTranslation();
  return (
    <Group justify="flex-end" align="center" gap="0.5rem">
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
  );
};

export default GoalsHeader;
