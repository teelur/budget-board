import { Stack } from "@mantine/core";
import { Button, BodyText, Tooltip } from "@teelur/budget-board-ui";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import { useTranslation } from "react-i18next";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const EnableAutoCategorizer = (): React.ReactNode => {
  const { t } = useTranslation();
  const { autoCategorizerModelOID, enableAutoCategorizer } = useUserSettings();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();

  const button = (
    <Button
      variant="filled"
      color={
        autoCategorizerModelOID !== null && !enableAutoCategorizer
          ? "error"
          : "primary"
      }
      size="compact-sm"
      disabled={autoCategorizerModelOID == null}
      loading={updateUserSettingsMutation.isPending}
      onClick={
        autoCategorizerModelOID != null
          ? () => {
              updateUserSettingsMutation.mutate({
                enableAutoCategorizer: !enableAutoCategorizer,
              } as IUserSettingsUpdateRequest);
            }
          : (event) => event.preventDefault() // Prevent click when disabled
      }
    >
      {enableAutoCategorizer ? t("enabled") : t("disabled")}
    </Button>
  );

  // If the button is disabled, we need to wrap it in a tooltip.
  const tooltip = (
    <Tooltip label={t("enable_auto_categorizer_button_disabled_hover")}>
      {button}
    </Tooltip>
  );

  return (
    <Stack gap={0}>
      <BodyText tone="primary" size="md" fw={500}>
        {t("enable_auto_categorizer")}
      </BodyText>
      <BodyText tone="secondary" size="sm">
        {t("enable_auto_categorizer_description")}
      </BodyText>
      <BodyText tone="primary" size="sm" fw={500}>
        {t("enable_auto_categorizer_warning")}
      </BodyText>
      {autoCategorizerModelOID == null ? tooltip : button}
    </Stack>
  );
};

export default EnableAutoCategorizer;
