import { ActionIcon, Tooltip } from "@mantine/core";
import { EyeIcon, EyeOffIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

const PrivacyModeButton = (): React.ReactNode => {
  const { t } = useTranslation();
  const { isPrivacyModeEnabled, togglePrivacyMode } = usePrivacyMode();

  const labelKey = isPrivacyModeEnabled
    ? "sensitive_values_hidden"
    : "sensitive_values_visible";
  const label = t(labelKey);
  const Icon = isPrivacyModeEnabled ? EyeOffIcon : EyeIcon;

  return (
    <Tooltip label={label}>
      <ActionIcon
        aria-label={label}
        onClick={togglePrivacyMode}
        size="lg"
        variant="subtle"
      >
        <Icon size={20} />
      </ActionIcon>
    </Tooltip>
  );
};

export default PrivacyModeButton;
