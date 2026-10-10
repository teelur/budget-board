import { Stack } from "@mantine/core";
import { BodyText, Button, Card, TextInput } from "@teelur/budget-board-ui";
import React from "react";
import { isNotEmpty, useField } from "@mantine/form";
import { useTranslation } from "react-i18next";
import { useUpdateApiKeyMutation } from "~/hooks/mutations/lunchFlow/useUpdateApiKeyMutation";

const LinkLunchFlow = (): React.ReactNode => {
  const { t } = useTranslation();
  const updateApiKeyMutation = useUpdateApiKeyMutation();

  const lunchFlowKeyField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("lunchflow_key_is_required")),
  });

  return (
    <Card>
      <Stack gap="0.5rem">
        <BodyText tone="secondary" size="sm">
          {t("link_lunchflow_description")}
        </BodyText>
        <Stack gap="0.5rem">
          <TextInput
            {...lunchFlowKeyField.getInputProps()}
            label={t("lunchflow_api_key")}
          />
          <Button
            variant="filled"
            color="primary"
            size="xs"
            loading={updateApiKeyMutation.isPending}
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
