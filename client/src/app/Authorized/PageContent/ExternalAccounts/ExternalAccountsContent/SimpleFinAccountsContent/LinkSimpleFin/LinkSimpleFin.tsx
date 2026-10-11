import { Stack } from "@mantine/core";
import { BodyText, Button, Card, TextInput } from "@teelur/budget-board-ui";
import React from "react";
import { isNotEmpty, useField } from "@mantine/form";
import { useTranslation } from "react-i18next";
import { useUpdateAccessTokenMutation } from "~/hooks/mutations/simpleFin/useUpdateAccessTokenMutation";

const LinkSimpleFin = (): React.ReactNode => {
  const { t } = useTranslation();
  const updateAccessTokenMutation = useUpdateAccessTokenMutation();

  const simpleFinKeyField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("simplefin_key_is_required")),
  });

  return (
    <Card>
      <Stack gap="0.5rem">
        <BodyText tone="secondary" size="sm">
          {t("link_simplefin_description")}
        </BodyText>
        <Stack gap="0.5rem">
          <TextInput
            {...simpleFinKeyField.getInputProps()}
            label={t("simplefin_access_token")}
          />
          <Button
            variant="filled"
            color="primary"
            size="xs"
            loading={updateAccessTokenMutation.isPending}
            onClick={() => {
              simpleFinKeyField.validate();

              if (simpleFinKeyField.getValue().length === 0) {
                return;
              }

              updateAccessTokenMutation.mutate(simpleFinKeyField.getValue());
            }}
          >
            {t("set_access_token")}
          </Button>
        </Stack>
      </Stack>
    </Card>
  );
};

export default LinkSimpleFin;
