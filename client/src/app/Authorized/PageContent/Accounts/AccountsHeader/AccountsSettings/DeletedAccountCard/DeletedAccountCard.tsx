import { ActionIcon, Badge, Group, LoadingOverlay, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IAccountResponse } from "~/models/account";
import ElevatedCard from "~/components/core/Card/ElevatedCard/ElevatedCard";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

interface DeletedAccountCardProps {
  account: IAccountResponse;
  institutionName?: string;
}

const DeletedAccountCard = (
  props: DeletedAccountCardProps
): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doRestoreAccount = useMutation({
    mutationFn: async () =>
      await request({
        url: `/api/account/restore`,
        method: "POST",
        params: { guid: props.account.id },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <ElevatedCard>
      <LoadingOverlay visible={doRestoreAccount.isPending} />
      <Group justify="space-between" wrap="nowrap">
        <Group gap="0.5rem">
          <Stack gap={0}>
            <PrimaryText size="sm">
              {props.account.name && props.account.name.length > 0
                ? props.account.name
                : t("unknown_account")}
            </PrimaryText>
            <DimmedText size="xs">
              {props.institutionName && props.institutionName.length > 0
                ? props.institutionName
                : t("unknown_institution")}
            </DimmedText>
          </Stack>
          {props.account.syncID !== null && (
            <Badge bg="blue">{t("simplefin")}</Badge>
          )}
        </Group>
        <Group style={{ alignSelf: "stretch" }}>
          <ActionIcon h="100%" onClick={() => doRestoreAccount.mutate()}>
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
        </Group>
      </Group>
    </ElevatedCard>
  );
};

export default DeletedAccountCard;
