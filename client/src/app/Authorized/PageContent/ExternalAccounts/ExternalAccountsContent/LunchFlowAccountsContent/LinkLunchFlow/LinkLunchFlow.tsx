import { Button, LoadingOverlay, Stack } from "@mantine/core";
import React from "react";
import { isNotEmpty, useField } from "@mantine/form";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useUpdateApiKeyMutation } from "~/hooks/mutations/lunchFlow/useRemoveApiKeyMutation";

const LinkLunchFlow = (): React.ReactNode => {
  const { t } = useTranslation();
  const updateApiKeyMutation = useUpdateApiKeyMutation();

  const lunchFlowKeyField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("lunchflow_key_is_required")),
  });

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={updateApiKeyMutation.isPending} zIndex={1000} />
      <Stack gap="0.5rem">
        <DimmedText size="sm">{t("link_lunchflow_description")}</DimmedText>
        <Stack gap="0.5rem">
          <TextInput
            {...lunchFlowKeyField.getInputProps()}
            label={
              <PrimaryText size="sm">{t("lunchflow_api_key")}</PrimaryText>
            }
            elevation={1}
          />
          <Button
            onClick={() => {
              lunchFlowKeyField.validate();

              if (lunchFlowKeyField.getValue().length === 0) {
                return;
              }

              updateApiKeyMutation.mutate(lunchFlowKeyField.getValue());
            }}
          >
            {t("set_api_key")}
          </Button>
        </Stack>
      </Stack>
    </Card>
  );
};

export default LinkLunchFlow;
