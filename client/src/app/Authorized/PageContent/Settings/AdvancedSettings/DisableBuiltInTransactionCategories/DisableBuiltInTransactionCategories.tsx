import { Button, Skeleton, Stack, Text } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";

const DisableBuiltInTransactionCategories = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

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
        color: "red",
        message: translateAxiosError(error),
      });
    },
  });

  if (userSettingsQuery.isPending) {
    return <Skeleton height={75} radius="md" />;
  }

  return (
    <Stack gap="0.25rem">
      <Text size="sm" fw={600}>
        Disable Built-In Transaction Categories
      </Text>
      <Text size="xs" c="dimmed">
        Disable the built-in transaction categories that come with Budget Board.
        This will hide them from all category selection dropdowns.
      </Text>
      <Button
        bg={
          userSettingsQuery.data?.disableBuiltInTransactionCategories
            ? ""
            : "red"
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
          ? "Enable"
          : "Disable"}
      </Button>
    </Stack>
  );
};

export default DisableBuiltInTransactionCategories;
