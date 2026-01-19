import { Badge, Button, Group, Stack } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import { AxiosError, AxiosResponse } from "axios";
import { notifications } from "@mantine/notifications";
import {
  lunchFlowAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import LinkLunchFlow from "./LinkLunchFlow/LinkLunchFlow";
import LunchFlowInstitutionCards from "./LunchFlowInstitutionCards/LunchFlowInstitutionCards";

const LunchFlowAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

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
  const doRemoveApiKey = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/lunchFlow/removeApiKey",
        method: "POST",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
      });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
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
          <PrimaryText size="lg">{t("lunchflow")}</PrimaryText>
          {userQuery.data?.lunchFlowApiKey && (
            <Badge color="var(--button-color-confirm)">{t("connected")}</Badge>
          )}
        </Group>
        {userQuery.data?.lunchFlowApiKey && (
          <Button
            bg="var(--button-color-destructive)"
            size="xs"
            loading={doRemoveApiKey.isPending}
            onClick={() => doRemoveApiKey.mutate()}
          >
            {t("remove_lunchflow")}
          </Button>
        )}
      </Group>
      {userQuery.data?.lunchFlowApiKey ? (
        <LunchFlowInstitutionCards />
      ) : (
        <LinkLunchFlow />
      )}
    </Stack>
  );
};

export default LunchFlowAccountsContent;
