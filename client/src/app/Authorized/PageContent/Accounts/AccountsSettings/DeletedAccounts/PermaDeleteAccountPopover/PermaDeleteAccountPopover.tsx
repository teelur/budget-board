import { ActionIcon, Button, Popover, Stack } from "@mantine/core";
import { Trash2Icon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { usePermanentDeleteAccountMutation } from "~/hooks/mutations/accounts/usePermanentDeleteAccountMutation";

interface PermaDeleteAccountPopoverProps {
  accountId: string;
}

const PermaDeleteAccountPopover = (
  props: PermaDeleteAccountPopoverProps,
): React.ReactNode => {
  const { t } = useTranslation();

  const permanentDeleteAccountMutation = usePermanentDeleteAccountMutation();
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
            loading={permanentDeleteAccountMutation.isPending}
            onClick={() =>
              permanentDeleteAccountMutation.mutate(props.accountId)
            }
          >
            {t("permanently_delete_account")}
          </Button>
        </Stack>
      </Popover.Dropdown>
    </Popover>
  );
};

export default PermaDeleteAccountPopover;
