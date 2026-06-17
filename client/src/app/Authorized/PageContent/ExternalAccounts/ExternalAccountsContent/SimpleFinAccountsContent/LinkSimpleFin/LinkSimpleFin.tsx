import { Button, LoadingOverlay, Stack } from "@mantine/core";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import {
  applicationUserQueryKey,
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { isNotEmpty, useField } from "@mantine/form";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import TextInput from "~/components/core/Input/TextInput/TextInput";

const LinkSimpleFin = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const simpleFinKeyField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("simplefin_key_is_required")),
  });

  const queryClient = useQueryClient();
  const doSetAccessToken = useMutation({
    mutationFn: async (setupToken: string) =>
      await request({
        url: "/api/simplefin/updateAccessToken",
        method: "PUT",
        params: { setupToken },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinAccountQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doSetAccessToken.isPending} zIndex={1000} />
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

              doSetAccessToken.mutate(simpleFinKeyField.getValue());
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
