import { Badge, Button, Group, Skeleton, Stack } from "@mantine/core";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import { notifications } from "@mantine/notifications";
import {
  accountsQueryKey,
  applicationUserQueryKey,
  institutionsQueryKey,
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import LinkLunchFlow from "./LinkLunchFlow/LinkLunchFlow";
import LunchFlowInstitutionCards from "./LunchFlowInstitutionCards/LunchFlowInstitutionCards";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";

const LunchFlowAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const applicationUserQuery = useApplicationUserQuery();

  const queryClient = useQueryClient();
  const doRemoveApiKey = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/lunchFlow/removeApiKey",
        method: "POST",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [applicationUserQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
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

    if (applicationUserQuery.data?.lunchFlowApiKey) {
      return <LunchFlowInstitutionCards />;
    }

    return <LinkLunchFlow />;
  };

  return (
    <Stack p={0} gap="0.5rem">
      <Group justify="space-between">
        <Group>
          <PrimaryHeading order={4}>{t("lunchflow")}</PrimaryHeading>
          {applicationUserQuery.data?.lunchFlowApiKey && (
            <Badge color="var(--button-color-confirm)">{t("connected")}</Badge>
          )}
        </Group>
        {applicationUserQuery.data?.lunchFlowApiKey && (
          <Button
            bg="var(--button-color-destructive)"
            size="xs"
            loading={doRemoveApiKey.isPending}
            disabled={
              doRemoveApiKey.isPending || applicationUserQuery.isPending
            }
            onClick={() => doRemoveApiKey.mutate()}
          >
            {t("remove_lunchflow")}
          </Button>
        )}
      </Group>
      {getContent()}
    </Stack>
  );
};

export default LunchFlowAccountsContent;
