import { Button, Group } from "@mantine/core";
import { PlusIcon, RotateCcwIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";

interface DashboardEditorProps {
  onDone: () => void;
  onAddWidget: () => void;
  onResetToDefaults: () => void;
  isResetting: boolean;
}

const DashboardEditor = ({
  onDone,
  onAddWidget,
  onResetToDefaults,
  isResetting,
}: DashboardEditorProps): React.ReactNode => {
  const { t } = useTranslation();
  const [isConfirmingReset, setIsConfirmingReset] = React.useState(false);

  const handleResetClick = () => {
    if (isConfirmingReset) {
      onResetToDefaults();
      setIsConfirmingReset(false);
    } else {
      setIsConfirmingReset(true);
    }
  };

  const handleCancelReset = () => {
    setIsConfirmingReset(false);
  };

  return (
    <Group justify="flex-end" gap="xs">
      <Button
        variant="subtle"
        leftSection={<PlusIcon size={16} />}
        onClick={onAddWidget}
      >
        {t("add_widget")}
      </Button>
      {isConfirmingReset ? (
        <Group gap="xs">
          <Button
            variant="outline"
            color="var(--button-color-destructive)"
            size="sm"
            loading={isResetting}
            onClick={handleResetClick}
          >
            {t("confirm_reset_to_defaults")}
          </Button>
          <Button variant="subtle" size="sm" onClick={handleCancelReset}>
            {t("cancel")}
          </Button>
        </Group>
      ) : (
        <Button
          variant="subtle"
          leftSection={<RotateCcwIcon size={16} />}
          onClick={handleResetClick}
          loading={isResetting}
        >
          {t("reset_to_defaults")}
        </Button>
      )}
      <Button onClick={onDone}>{t("done_editing")}</Button>
    </Group>
  );
};

export default DashboardEditor;
