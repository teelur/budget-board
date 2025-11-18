import {
  Button,
  Card,
  Badge,
  Group,
  LoadingOverlay,
  PasswordInput,
  Stack,
  Text,
  Skeleton,
} from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import { AxiosError, AxiosResponse } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { isNotEmpty, useField } from "@mantine/form";

const LinkSimpleFin = (): React.ReactNode => {
  const simpleFinKeyField = useField<string>({
    initialValue: "",
    validate: isNotEmpty("SimpleFin key is required"),
  });

  const { request } = React.useContext<any>(AuthContext);

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
    mutationFn: async (setupToken: string) =>
      await request({
        url: "/api/simplefin/updateAccessToken",
        method: "PUT",
        params: { setupToken },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      notifications.show({
        color: "green",
        message: "SimpleFin account linked!",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "red",
        message: translateAxiosError(error),
      });
    },
  });

  if (userQuery.isLoading) {
    return <Skeleton height={150} radius="md" />;
  }

  return (
    <Card p="0.5rem" radius="md" shadow="sm" withBorder>
      <LoadingOverlay visible={doSetAccessToken.isPending} zIndex={1000} />
      <Stack gap="1rem">
        <Group gap="1rem">
          <Text fw={700} size="lg">
            Link SimpleFIN
          </Text>
          {userQuery.data?.accessToken && (
            <Badge color="green" maw={80}>
              Linked
            </Badge>
          )}
        </Group>
        <Stack gap="0.5rem">
          <PasswordInput
            {...simpleFinKeyField.getInputProps()}
            label={
              <Text fw={600} size="sm">
                SimpleFin Access Token
              </Text>
            }
          />
          <Button
            onClick={() => {
              simpleFinKeyField.validate();
              if (!simpleFinKeyField.error) {
                doSetAccessToken.mutate(simpleFinKeyField.getValue());
              }
            }}
          >
            Save Changes
          </Button>
        </Stack>
      </Stack>
    </Card>
  );
};

export default LinkSimpleFin;
