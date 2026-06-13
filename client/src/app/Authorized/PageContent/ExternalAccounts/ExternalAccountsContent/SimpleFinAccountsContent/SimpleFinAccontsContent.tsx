import { Badge, Button, Group, Stack } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import { AxiosError, AxiosResponse } from "axios";
import SimpleFinOrganizationCards from "./SimpleFinOrganizationCards/SimpleFinOrganizationCards";
import { notifications } from "@mantine/notifications";
import {
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  translateAxiosError,
  userQueryKey,
} from "~/helpers/requests";
import LinkSimpleFin from "./LinkSimpleFin/LinkSimpleFin";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { accountsQueryKey, institutionsQueryKey } from "~/helpers/requests";

const SimpleFinAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const userQuery = useQuery({
    queryKey: [userQueryKey],
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
  const doRemoveAccessToken = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/simplefin/removeAccessToken",
        method: "POST",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [userQueryKey] });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinOrganizationQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinAccountQueryKey],
      });
      await queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] });
      await queryClient.invalidateQueries({ queryKey: [accountsQueryKey] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <Stack p={0} gap="0.5rem">
      <Group justify="space-between">
        <Group>
          <PrimaryHeading order={4}>{t("simplefin")}</PrimaryHeading>
          {userQuery.data?.simpleFinAccessToken && (
            <Badge color="var(--button-color-confirm)">{t("connected")}</Badge>
          )}
        </Group>
        {userQuery.data?.simpleFinAccessToken && (
          <Button
            bg="var(--button-color-destructive)"
            size="xs"
            loading={doRemoveAccessToken.isPending}
            onClick={() => doRemoveAccessToken.mutate()}
          >
            {t("remove_simplefin")}
          </Button>
        )}
      </Group>
      {userQuery.data?.simpleFinAccessToken ? (
        <SimpleFinOrganizationCards />
      ) : (
        <LinkSimpleFin />
      )}
    </Stack>
  );
};

export default SimpleFinAccountsContent;
