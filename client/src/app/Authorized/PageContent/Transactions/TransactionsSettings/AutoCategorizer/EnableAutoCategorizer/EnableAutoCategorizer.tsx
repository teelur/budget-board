import { Button, LoadingOverlay, Stack, Tooltip } from "@mantine/core";
import React from "react";
import { IUserSettingsUpdateRequest } from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

const EnableAutoCategorizer = (): React.ReactNode => {
  const { t } = useTranslation();
  const { autoCategorizerModelOID, enableAutoCategorizer } = useUserSettings();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();

  const button = (
    <Button
      variant="primary"
      bg={
        autoCategorizerModelOID == null
          ? ""
          : enableAutoCategorizer
            ? ""
            : "var(--button-color-destructive)"
      }
      size="xs"
      onClick={
        autoCategorizerModelOID != null
          ? () => {
              updateUserSettingsMutation.mutate({
                enableAutoCategorizer: !enableAutoCategorizer,
              } as IUserSettingsUpdateRequest);
            }
          : (event) => event.preventDefault() // Prevent click when disabled
      }
      disabled={autoCategorizerModelOID == null}
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
    <Stack gap="0.25rem">
      <LoadingOverlay visible={updateUserSettingsMutation.isPending} />
      <PrimaryText size="sm">{t("enable_auto_categorizer")}</PrimaryText>
      <DimmedText size="xs">
        {t("enable_auto_categorizer_description")}
      </DimmedText>
      <DimmedText size="xs">{t("enable_auto_categorizer_warning")}</DimmedText>
      {autoCategorizerModelOID == null ? tooltip : button}
    </Stack>
  );
};

export default EnableAutoCategorizer;
