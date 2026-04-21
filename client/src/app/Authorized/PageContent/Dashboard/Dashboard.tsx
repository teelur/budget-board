import { Stack } from "@mantine/core";
import React from "react";
import DashboardFooter from "./DashboardFooter/DashboardFooter";
import DashboardHeader from "./DashboardHeader/DashboardHeader";
import DashboardContent from "./DashboardContent/DashboardContent";

const Dashboard = (): React.ReactNode => {
  const [isEditMode, setIsEditMode] = React.useState(false);

  return (
    <Stack w="100%" flex="1" justify="space-between">
      <Stack gap="0.25rem">
        <DashboardHeader
          isEditMode={isEditMode}
          setIsEditMode={setIsEditMode}
        />
        <DashboardContent isEditMode={isEditMode} />
      </Stack>
      <DashboardFooter />
    </Stack>
  );
};

export default Dashboard;
