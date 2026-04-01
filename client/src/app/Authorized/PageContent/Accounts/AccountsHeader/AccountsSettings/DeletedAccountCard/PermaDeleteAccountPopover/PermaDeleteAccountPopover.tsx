import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  lunchFlowAccountQueryKey,
  simpleFinAccountQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { ActionIcon, Button, Popover, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Trash2Icon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface PermaDeleteAccountPopoverProps {
  accountId: string;
}

const PermaDeleteAccountPopover = (
  props: PermaDeleteAccountPopoverProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const queryClient = useQueryClient();

  const doPermaDeleteAccount = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/account/permanentDelete",
        method: "DELETE",
        params: { guid: props.accountId },
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({
        queryKey: [simpleFinAccountQueryKey],
      });
      await queryClient.invalidateQueries({
        queryKey: [lunchFlowAccountQueryKey],
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });
  return (
    <Popover>
      <Popover.Target>
        <ActionIcon h="100%" bg={"var(--button-color-destructive)"}>
          <Trash2Icon size="1.2rem" />
        </ActionIcon>
      </Popover.Target>
      <Popover.Dropdown maw={350}>
        <Stack gap={10}>
          <PrimaryText size="xs">
            {t("perma_delete_account_warning")}
          </PrimaryText>
          <Button
            size="xs"
            color="var(--button-color-destructive)"
            loading={doPermaDeleteAccount.isPending}
            onClick={() => doPermaDeleteAccount.mutate()}
          >
            {t("permanently_delete_account")}
          </Button>
        </Stack>
      </Popover.Dropdown>
    </Popover>
  );
};

export default PermaDeleteAccountPopover;
