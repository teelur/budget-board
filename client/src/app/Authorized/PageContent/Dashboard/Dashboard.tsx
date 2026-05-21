import { Stack } from "@mantine/core";
import React from "react";
import DashboardFooter from "./DashboardFooter/DashboardFooter";
import DashboardHeader from "./DashboardHeader/DashboardHeader";
import DashboardMobileHeader from "./DashboardMobileHeader/DashboardMobileHeader";
import DashboardContent from "./DashboardContent/DashboardContent";
import useIsMobile from "~/hooks/useIsMobile";

const Dashboard = (): React.ReactNode => {
  const [isEditMode, setIsEditMode] = React.useState(false);
  const [editTarget, setEditTarget] = React.useState<"lg" | "sm">("lg");
  const isMobile = useIsMobile();

  const effectiveEditTarget: "lg" | "sm" = isMobile ? "sm" : editTarget;

  return (
    <Stack w="100%" maw={1400} flex="1" align="center" justify="space-between">
      <Stack w="100%" gap="0.25rem" justify="center">
        {isMobile ? (
          <DashboardMobileHeader
            isEditMode={isEditMode}
            setIsEditMode={setIsEditMode}
          />
        ) : (
          <DashboardHeader
            isEditMode={isEditMode}
            setIsEditMode={setIsEditMode}
            editTarget={editTarget}
            setEditTarget={setEditTarget}
          />
        )}
        <DashboardContent
          isEditMode={isEditMode}
          editTarget={effectiveEditTarget}
        />
      </Stack>
      <DashboardFooter />
    </Stack>
  );
};

export default Dashboard;
