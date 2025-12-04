import { LoadingOverlay, Skeleton, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  Currency,
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";

const UserSettings = (): React.ReactNode => {
  const currencyField = useField({
    initialValue: "",
  });

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

  React.useEffect(() => {
    if (userSettingsQuery.data) {
      currencyField.setValue(userSettingsQuery.data.currency);
    }
  }, [userSettingsQuery.data]);

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      <Stack gap="0.5rem">
        <PrimaryText size="lg">User Settings</PrimaryText>
        <Stack gap="0.5rem">
          {userSettingsQuery.isPending ? (
            <Skeleton h={60} radius="md" />
          ) : (
            <Select
              label={<PrimaryText size="sm">Preferred Currency</PrimaryText>}
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
              elevation={1}
            />
          )}
        </Stack>
      </Stack>
    </Card>
  );
};

export default UserSettings;
