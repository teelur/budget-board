import {
  Card,
  LoadingOverlay,
  Select,
  Skeleton,
  Stack,
  Title,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  Currency,
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";

const UserSettings = (): React.ReactNode => {
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

  const currencyField = useField({
    initialValue: userSettingsQuery.data?.currency || "",
  });

  React.useEffect(() => {
    if (userSettingsQuery.data) {
      currencyField.setValue(userSettingsQuery.data.currency);
    }
  }, [userSettingsQuery.data]);

  return (
    <Card>
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      <Stack>
        <Title order={3}>User Settings</Title>
        {userSettingsQuery.isPending ? (
          <Skeleton h={60} />
        ) : (
          <Select
            label="Currency"
            placeholder="Select currency"
            data={Object.values(Currency).map((currency) => ({
              value: currency,
              label: currency,
            }))}
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
    </Card>
  );
};

export default UserSettings;
