import { AuthContext } from "~/providers/AuthProvider/AuthProvider";
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

  const { request } = React.useContext<any>(AuthContext);

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
      notifications.show({ color: "red", message: translateAxiosError(error) });
    },
  });
  return (
    <Popover>
      <Popover.Target>
        <ActionIcon h="100%" size="sm" color="red">
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
            color="red"
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
