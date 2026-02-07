import { Button, Skeleton, Stack, Tooltip } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

const EnableAutoCategorizer = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const queryClient = useQueryClient();
  const doUpdateUserSettings = useMutation({
    mutationFn: async (updatedUserSettings: IUserSettingsUpdateRequest) =>
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: updatedUserSettings,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["userSettings"] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  if (userSettingsQuery.isPending) {
    return <Skeleton height={75} radius="md" />;
  }

  const button = <Button
          bg={
            userSettingsQuery.data?.enableAutoCategorizer
              ? ""
              : "var(--button-color-destructive)"
          }
          variant="primary"
          size="xs"
          onClick={
            userSettingsQuery.data?.autoCategorizerModelOID != null
            ? () => {
            doUpdateUserSettings.mutate({
              enableAutoCategorizer:
                !userSettingsQuery.data?.enableAutoCategorizer,
              } as IUserSettingsUpdateRequest);
            }
            : (event) => event.preventDefault() // Prevent click when disabled
        }
          disabled={ userSettingsQuery.data?.autoCategorizerModelOID == null }
        >
          {userSettingsQuery.data?.enableAutoCategorizer
            ? t("enabled")
            : t("disabled")}
        </Button>;

  // If the button is disabled, we need to wrap it in a tooltip.
  const tooltip = <Tooltip label={t("enable_auto_categorizer_button_disabled_hover")}>
        {button}
      </Tooltip>

  return (
    <Stack gap="0.25rem">
      <PrimaryText size="sm">
        {t("enable_auto_categorizer")}
      </PrimaryText>
      <DimmedText size="xs">
        {t("enable_auto_categorizer_description")}
      </DimmedText>
      <DimmedText size="xs">
        {t("enable_auto_categorizer_warning")}
      </DimmedText>
      {userSettingsQuery.data?.autoCategorizerModelOID == null
              ? tooltip
              : button
      }
    </Stack>
  );
};

export default EnableAutoCategorizer;
