import classes from "./GoalsHeader.module.css";

import { Button, Group } from "@mantine/core";
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
    <Group className={classes.root}>
      <Button
        variant="outline"
        color={
          props.includeInterest
            ? "var(--button-color-confirm)"
            : "var(--button-color-destructive)"
        }
        onClick={props.toggleIncludeInterest}
      >
        {t("include_interest")}
      </Button>
      <AddGoalModal />
    </Group>
  );
};

export default GoalsHeader;
