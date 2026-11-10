import { Stack } from "@mantine/core";
import { BodyText, Button, Code, Modal } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";

export interface TemporaryPassword {
  username: string;
  password: string;
}

interface TemporaryPasswordModalProps {
  temporaryPassword: TemporaryPassword | null;
  onClose: () => void;
}

const TemporaryPasswordModal = ({
  temporaryPassword,
  onClose,
}: TemporaryPasswordModalProps): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Modal
      opened={temporaryPassword !== null}
      onClose={onClose}
      title={t("temporary_password")}
    >
      <Stack gap="0.5rem">
        <BodyText tone="secondary" size="sm">
          {t("temporary_password_for_user", {
            username: temporaryPassword?.username ?? "",
          })}
        </BodyText>
        {temporaryPassword && <Code block>{temporaryPassword.password}</Code>}
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          fullWidth
          onClick={onClose}
        >
          {t("close")}
        </Button>
      </Stack>
    </Modal>
  );
};

export default TemporaryPasswordModal;
