import { Button, LoadingOverlay, Stack } from "@mantine/core";
import React from "react";
import { isNotEmpty, useField } from "@mantine/form";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useUpdateAccessTokenMutation } from "~/hooks/mutations/simpleFin/useUpdateAccessTokenMutation";

const LinkSimpleFin = (): React.ReactNode => {
  const { t } = useTranslation();
  const updateAccessTokenMutation = useUpdateAccessTokenMutation();

  const simpleFinKeyField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("simplefin_key_is_required")),
  });

  return (
    <Card elevation={1}>
      <LoadingOverlay
        visible={updateAccessTokenMutation.isPending}
        zIndex={1000}
      />
      <Stack gap="0.5rem">
        <DimmedText size="sm">{t("link_simplefin_description")}</DimmedText>
        <Stack gap="0.5rem">
          <TextInput
            {...simpleFinKeyField.getInputProps()}
            label={
              <PrimaryText size="sm">{t("simplefin_access_token")}</PrimaryText>
            }
            elevation={1}
          />
          <Button
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
