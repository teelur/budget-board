import { Group } from "@mantine/core";
import { Button, type ButtonColor } from "@teelur/budget-board-ui";
import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";

interface ModalActionsProps {
  actionColor?: ButtonColor;
  actionDisabled?: boolean;
  actionLabel: ReactNode;
  isPending?: boolean;
  onAction: () => void;
  onCancel: () => void;
}

export const ModalActions = ({
  actionColor = "primary",
  actionDisabled,
  actionLabel,
  isPending,
  onAction,
  onCancel,
}: ModalActionsProps): ReactNode => {
  const { t } = useTranslation();

  return (
    <Group w="100%" gap="0.5rem">
      <Button
        variant="filled"
        color="neutral"
        size="compact-sm"
        flex="1 1 auto"
        disabled={isPending}
        onClick={onCancel}
      >
        {t("cancel")}
      </Button>
      <Button
        variant="filled"
        color={actionColor}
        size="compact-sm"
        flex="1 1 auto"
        disabled={actionDisabled}
        loading={isPending}
        onClick={onAction}
      >
        {actionLabel}
      </Button>
    </Group>
  );
};
