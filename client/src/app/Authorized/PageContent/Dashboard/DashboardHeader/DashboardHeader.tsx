import { Button, Group, SegmentedControl } from "@mantine/core";
import DashboardEditor from "./DashboardEditor/DashboardEditor";
import { LayoutIcon } from "lucide-react";
import React from "react";
import { useDisclosure } from "@mantine/hooks";
import WidgetPicker from "./WidgetPicker/WidgetPicker";
import { useTranslation } from "react-i18next";

interface DashboardHeaderProps {
  isEditMode: boolean;
  setIsEditMode: React.Dispatch<React.SetStateAction<boolean>>;
  editTarget: "lg" | "sm";
  setEditTarget: React.Dispatch<React.SetStateAction<"lg" | "sm">>;
}

const DashboardHeader = ({
  isEditMode,
  setIsEditMode,
  editTarget,
  setEditTarget,
}: DashboardHeaderProps): React.ReactNode => {
  const [pickerOpened, { open: openPicker, close: closePicker }] =
    useDisclosure(false);

  const { t } = useTranslation();

  return (
    <>
      <Group justify="space-between" align="center">
        <Group>
          {isEditMode && (
            <SegmentedControl
              value={editTarget}
              onChange={(v) => setEditTarget(v as "lg" | "sm")}
              size="sm"
              data={[
                { label: t("desktop"), value: "lg" },
                { label: t("mobile"), value: "sm" },
              ]}
            />
          )}
        </Group>
        <Group>
          {isEditMode ? (
            <DashboardEditor
              onDone={() => setIsEditMode(false)}
              onAddWidget={openPicker}
              editTarget={editTarget}
            />
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
      <WidgetPicker opened={pickerOpened} onClose={closePicker} />
    </>
  );
};

export default DashboardHeader;
