import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { ActionIcon, Button, Checkbox, Popover, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { Trash2Icon } from "lucide-react";
import React from "react";

interface DeleteAccountPopoverProps {
  accountId: string;
}

const DeleteAccountPopover = (
  props: DeleteAccountPopoverProps
): React.ReactNode => {
  const [deleteTransactions, { toggle }] = useDisclosure(false);

  const { request } = useAuth();

  const queryClient = useQueryClient();

  const doDeleteAccount = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/account",
        method: "DELETE",
        params: { guid: props.accountId, deleteTransactions },
      }),
    onSuccess: async () => {
      // Refetch the accounts and institutions queries immediatly after the account is deleted
      await queryClient.refetchQueries({ queryKey: ["institutions"] });
      await queryClient.refetchQueries({ queryKey: ["accounts"] });
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
        <ActionIcon h="100%" size="sm" color="var(--button-color-destructive)">
          <Trash2Icon size={16} />
        </ActionIcon>
      </Popover.Target>
      <Popover.Dropdown>
        <Stack gap={10}>
          <Checkbox
            checked={deleteTransactions}
            onChange={toggle}
            label="Delete Transactions?"
          />
          <Button
            color="var(--button-color-destructive)"
            loading={doDeleteAccount.isPending}
            onClick={() => doDeleteAccount.mutate()}
          >
            Delete
          </Button>
        </Stack>
      </Popover.Dropdown>
    </Popover>
  );
};

export default DeleteAccountPopover;
