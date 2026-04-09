import { Button, Skeleton, Stack } from "@mantine/core";
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

const DisableBuiltInTransactionCategories = (): React.ReactNode => {
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

  return (
    <Stack gap="0.25rem">
      <PrimaryText size="sm">
        {t("built_in_transaction_categories")}
      </PrimaryText>
      <DimmedText size="xs">
        {t("disable_built_in_transaction_categories_description")}
      </DimmedText>
      <DimmedText size="xs">
        {t("disable_built_in_transaction_categories_warning")}
      </DimmedText>
      <Button
        bg={
          userSettingsQuery.data?.disableBuiltInTransactionCategories
            ? "var(--button-color-destructive)"
            : ""
        }
        variant="primary"
        size="xs"
        loading={doUpdateUserSettings.isPending}
        onClick={() => {
          doUpdateUserSettings.mutate({
            disableBuiltInTransactionCategories:
              !userSettingsQuery.data?.disableBuiltInTransactionCategories,
          } as IUserSettingsUpdateRequest);
        }}
      >
        {userSettingsQuery.data?.disableBuiltInTransactionCategories
          ? t("disabled")
          : t("enabled")}
      </Button>
    </Stack>
  );
};

export default DisableBuiltInTransactionCategories;
