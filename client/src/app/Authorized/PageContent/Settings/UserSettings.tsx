import classes from "./Settings.module.css";

import {
  Card,
  CardSection,
  LoadingOverlay,
  Select,
  Skeleton,
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

  if (userSettingsQuery.isPending) {
    return <Skeleton h={141} radius="md" className={classes.skeleton} />;
  }

  return (
    <Card className={classes.card} withBorder radius="md" shadow="sm">
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      <CardSection className={classes.cardSection}>
        <Title order={3}>User Settings</Title>
      </CardSection>
      <CardSection className={classes.cardSection}>
        {userSettingsQuery.isPending ? (
          <Skeleton h={60} />
        ) : (
          <Select
            label="Currency"
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
      </CardSection>
    </Card>
  );
};

export default UserSettings;
