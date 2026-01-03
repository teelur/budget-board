import { Button, Group, Stack } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import { AxiosError, AxiosResponse } from "axios";
import SimpleFinOrganizationCards from "./SimpleFinOrganizationCards/SimpleFinOrganizationCards";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import LinkSimpleFin from "./LinkSimpleFin/LinkSimpleFin";

const SimpleFinAccountsContent = (): React.ReactNode => {
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
  const doRemoveAccessToken = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/simplefin/removeAccessToken",
        method: "POST",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      await queryClient.invalidateQueries({
        queryKey: ["simplefinOrganizations"],
      });
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
        <PrimaryText size="lg">{t("simplefin")}</PrimaryText>
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
