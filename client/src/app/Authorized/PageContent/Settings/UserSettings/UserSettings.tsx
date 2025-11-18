import {
  Card,
  LoadingOverlay,
  Select,
  Skeleton,
  Stack,
  Text,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  Currency,
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";

const UserSettings = (): React.ReactNode => {
  const currencyField = useField({
    initialValue: "",
  });

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

  React.useEffect(() => {
    if (userSettingsQuery.data) {
      currencyField.setValue(userSettingsQuery.data.currency);
    }
  }, [userSettingsQuery.data]);

  return (
    <Card p="0.5rem" radius="md" shadow="sm" withBorder>
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      <Stack gap="1rem">
        <Text fw={700} size="lg">
          User Settings
        </Text>
        <Stack gap="0.5rem">
          {userSettingsQuery.isPending ? (
            <Skeleton h={60} radius="md" />
          ) : (
            <Select
              label={
                <Text fw={600} size="sm">
                  Preferred Currency
                </Text>
              }
              placeholder="Select currency"
              searchable
              nothingFoundMessage="No currencies found"
              data={Intl.supportedValuesOf("currency")}
              {...currencyField.getInputProps()}
              onChange={(value) => {
                if (value) {
                  doUpdateUserSettings.mutate({
                    currency: value as Currency,
                  });
                }
              }}
            />
          )}
        </Stack>
      </Stack>
    </Card>
  );
};

export default UserSettings;
