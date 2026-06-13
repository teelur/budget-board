import { ActionIcon, Badge, Group, LoadingOverlay, Stack } from "@mantine/core";
import { Undo2Icon } from "lucide-react";
import React from "react";
import { IAccountResponse } from "~/models/account";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import PermaDeleteAccountPopover from "./PermaDeleteAccountPopover/PermaDeleteAccountPopover";
import Card from "~/components/core/Card/Card";
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
    <Card elevation={1}>
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
          <Badge bg="blue">{t(props.account.source)}</Badge>
        </Group>
        <Group style={{ alignSelf: "stretch" }} wrap="nowrap" gap="0.5rem">
          <ActionIcon
            h="100%"
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
