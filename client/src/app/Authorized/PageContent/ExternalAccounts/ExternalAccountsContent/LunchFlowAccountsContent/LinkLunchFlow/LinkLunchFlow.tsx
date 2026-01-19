import { Button, LoadingOverlay, Stack, Skeleton } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import { AxiosError, AxiosResponse } from "axios";
import {
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { isNotEmpty, useField } from "@mantine/form";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import TextInput from "~/components/core/Input/TextInput/TextInput";

const LinkLunchFlow = (): React.ReactNode => {
  const { t } = useTranslation();

  const lunchFlowKeyField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("lunchflow_key_is_required")),
  });

  const { request } = useAuth();

  const userQuery = useQuery({
    queryKey: ["user"],
    queryFn: async (): Promise<IApplicationUser | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/applicationUser",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IApplicationUser;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doSetAccessToken = useMutation({
    mutationFn: async (apiKey: string) =>
      await request({
        url: "/api/lunchflow/updateApiKey",
        method: "PUT",
        params: { apiKey },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  if (userQuery.isLoading) {
    return <Skeleton height={150} radius="md" />;
  }

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doSetAccessToken.isPending} zIndex={1000} />
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

              doSetAccessToken.mutate(lunchFlowKeyField.getValue());
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
