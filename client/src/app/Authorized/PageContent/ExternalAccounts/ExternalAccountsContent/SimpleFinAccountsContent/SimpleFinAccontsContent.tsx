import { Badge, Button, Group, Skeleton, Stack } from "@mantine/core";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import SimpleFinOrganizationCards from "./SimpleFinOrganizationCards/SimpleFinOrganizationCards";
import { notifications } from "@mantine/notifications";
import {
  accountsQueryKey,
  applicationUserQueryKey,
  institutionsQueryKey,
  simpleFinAccountQueryKey,
  simpleFinOrganizationQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import LinkSimpleFin from "./LinkSimpleFin/LinkSimpleFin";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";

const SimpleFinAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const applicationUserQuery = useApplicationUserQuery();

  const queryClient = useQueryClient();
  const doRemoveAccessToken = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/simplefin/removeAccessToken",
        method: "POST",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
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

  const getContent = () => {
    if (applicationUserQuery.isPending) {
      return <Skeleton height={150} radius="md" />;
    }

    if (applicationUserQuery.data?.simpleFinAccessToken) {
      return <SimpleFinOrganizationCards />;
    }

    return <LinkSimpleFin />;
  };

  return (
    <Stack p={0} gap="0.5rem">
      <Group justify="space-between">
        <Group>
          <PrimaryHeading order={4}>{t("simplefin")}</PrimaryHeading>
          {applicationUserQuery.data?.simpleFinAccessToken && (
            <Badge color="var(--button-color-confirm)">{t("connected")}</Badge>
          )}
        </Group>
        {applicationUserQuery.data?.simpleFinAccessToken && (
          <Button
            bg="var(--button-color-destructive)"
            size="xs"
            loading={doRemoveAccessToken.isPending}
            disabled={
              doRemoveAccessToken.isPending || applicationUserQuery.isPending
            }
            onClick={() => doRemoveAccessToken.mutate()}
          >
            {t("remove_simplefin")}
          </Button>
        )}
      </Group>
      {getContent()}
    </Stack>
  );
};

export default SimpleFinAccountsContent;
