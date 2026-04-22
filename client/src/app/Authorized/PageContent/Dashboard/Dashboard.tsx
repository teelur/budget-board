import { Stack } from "@mantine/core";
import React from "react";
import DashboardFooter from "./DashboardFooter/DashboardFooter";
import DashboardHeader from "./DashboardHeader/DashboardHeader";
import DashboardContent from "./DashboardContent/DashboardContent";

const Dashboard = (): React.ReactNode => {
  const [isEditMode, setIsEditMode] = React.useState(false);
  const [currentBreakpoint, setCurrentBreakpoint] =
    React.useState<string>("lg");

  const effectiveEditMode = isEditMode && currentBreakpoint !== "sm";

  return (
    <Stack w="100%" maw={1400} flex="1" justify="space-between">
      <Stack gap="0.25rem">
        {currentBreakpoint !== "sm" && (
          <DashboardHeader
            isEditMode={effectiveEditMode}
            setIsEditMode={setIsEditMode}
          />
        )}
        <DashboardContent
          isEditMode={effectiveEditMode}
          onBreakpointChange={setCurrentBreakpoint}
        />
      </Stack>
      <DashboardFooter />
    </Stack>
  );
};

export default Dashboard;
