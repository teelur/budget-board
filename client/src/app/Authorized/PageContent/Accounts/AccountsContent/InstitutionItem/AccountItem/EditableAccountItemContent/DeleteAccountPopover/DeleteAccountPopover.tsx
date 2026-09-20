import { ActionIcon, Checkbox, Popover, Stack } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { useDisclosure } from "@mantine/hooks";
import { Trash2Icon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useDeleteAccountMutation } from "~/hooks/mutations/accounts/useDeleteAccountMutation";

interface DeleteAccountPopoverProps {
  accountId: string;
}

const DeleteAccountPopover = (
  props: DeleteAccountPopoverProps,
): React.ReactNode => {
  const [deleteTransactions, { toggle }] = useDisclosure(false);

  const { t } = useTranslation();

  const deleteAccountMutation = useDeleteAccountMutation();

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
            label={
              <PrimaryText size="sm">{t("delete_transactions")}</PrimaryText>
            }
          />
          <Button
            variant="filled"
            color="error"
            size="compact-xs"
            loading={deleteAccountMutation.isPending}
            onClick={() =>
              deleteAccountMutation.mutate({
                accountId: props.accountId,
                deleteTransactions,
              })
            }
          >
            {t("delete_account")}
          </Button>
        </Stack>
      </Popover.Dropdown>
    </Popover>
  );
};

export default DeleteAccountPopover;
