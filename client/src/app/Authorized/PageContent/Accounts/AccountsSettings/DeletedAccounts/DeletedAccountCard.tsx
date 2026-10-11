import { Group, Stack } from "@mantine/core";
import { ActionIcon, Badge, BodyText, Card } from "@teelur/budget-board-ui";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { IAccountResponse } from "~/models/account";
import { useTranslation } from "react-i18next";
import PermaDeleteAccountPopover from "./PermaDeleteAccountPopover/PermaDeleteAccountPopover";
import { useRestoreAccountMutation } from "~/hooks/mutations/accounts/useRestoreAccountMutation";

interface DeletedAccountCardProps {
  account: IAccountResponse;
  institutionName?: string;
}

const DeletedAccountCard = (
  props: DeletedAccountCardProps,
): React.ReactNode => {
  const { t } = useTranslation();

  const doRestoreAccount = useRestoreAccountMutation();

  return (
    <Card>
      <Group justify="space-between" wrap="nowrap">
        <Group gap="0.5rem">
          <Stack gap={0}>
            <BodyText component="span" tone="primary" size="sm" fw={500}>
              {props.account.name && props.account.name.length > 0
                ? props.account.name
                : t("unknown_account")}
            </BodyText>
            <BodyText component="span" tone="secondary" size="xs">
              {props.institutionName && props.institutionName.length > 0
                ? props.institutionName
                : t("unknown_institution")}
            </BodyText>
          </Stack>
          <Badge variant="filled" color="primary" size="xs">
            {t(props.account.source)}
          </Badge>
        </Group>
        <Group style={{ alignSelf: "stretch" }} wrap="nowrap" gap="0.5rem">
          <ActionIcon
            variant="outline"
            color="primary"
            size="compact-sm"
            h="100%"
            loading={doRestoreAccount.isPending}
            onClick={() => doRestoreAccount.mutate(props.account.id)}
          >
            <Undo2Icon size="1.2rem" />
          </ActionIcon>
          <PermaDeleteAccountPopover accountId={props.account.id} />
        </Group>
      </Group>
    </Card>
  );
};

export default DeletedAccountCard;
