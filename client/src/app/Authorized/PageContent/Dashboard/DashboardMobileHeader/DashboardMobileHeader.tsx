import { Badge, Button, Group } from "@mantine/core";
import { LayoutIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useResetSmallScreenLayoutMutation } from "~/hooks/mutations/widgetSettings/useResetSmallScreenLayoutMutation";

interface DashboardMobileHeaderProps {
  isEditMode: boolean;
  setIsEditMode: React.Dispatch<React.SetStateAction<boolean>>;
}

const DashboardMobileHeader = ({
  isEditMode,
  setIsEditMode,
}: DashboardMobileHeaderProps): React.ReactNode => {
  const { t } = useTranslation();
  const resetSmallScreenLayoutMutation = useResetSmallScreenLayoutMutation();

  return (
    <Group justify="space-between" align="center">
      <Group>
        {isEditMode && <Badge variant="light">{t("mobile")}</Badge>}
      </Group>
      <Group gap={"0.5rem"}>
        {isEditMode ? (
          <>
            <Button
              variant="subtle"
              size="xs"
              loading={resetSmallScreenLayoutMutation.isPending}
              onClick={() => resetSmallScreenLayoutMutation.mutate()}
            >
              {t("reset_to_desktop_order")}
            </Button>
            <Button size="xs" onClick={() => setIsEditMode(false)}>
              {t("done_editing")}
            </Button>
          </>
        ) : (
          <Button
            size="xs"
            variant="subtle"
            leftSection={<LayoutIcon size={16} />}
            onClick={() => setIsEditMode(true)}
          >
            {t("edit_layout")}
          </Button>
        )}
      </Group>
    </Group>
  );
};

export default DashboardMobileHeader;
