import { Group, Stack } from "@mantine/core";
import { Badge, BodyText, Button, Table } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import type { IManagedUserResponse } from "~/models/managedUser";

interface ManagedUserRowProps {
  user: IManagedUserResponse;
  isEnablePending: boolean;
  onResetPassword: (user: IManagedUserResponse) => void;
  onEnable: (user: IManagedUserResponse) => void;
  onDisable: (user: IManagedUserResponse) => void;
  onTransferSuperuser: (user: IManagedUserResponse) => void;
}

const ManagedUserRow = ({
  user,
  isEnablePending,
  onResetPassword,
  onEnable,
  onDisable,
  onTransferSuperuser,
}: ManagedUserRowProps): React.ReactNode => {
  const { t } = useTranslation();
  const loginMethods = [
    user.hasPassword ? t("local_login") : null,
    user.hasOidcLogin ? t("oidc_login") : null,
  ].filter((method): method is string => method !== null);

  return (
    <Table.Tr>
      <Table.Td>
        <Stack gap={"0.25rem"}>
          <BodyText tone="primary" size="sm" fw={600}>
            {user.username}
          </BodyText>
          {user.email && (
            <BodyText size="xs" tone="secondary">
              {user.email}
            </BodyText>
          )}
        </Stack>
      </Table.Td>
      <Table.Td>{loginMethods.join(", ") || t("none")}</Table.Td>
      <Table.Td>
        <Group gap="0.25rem" wrap="wrap">
          <Badge
            variant="filled"
            size="xs"
            color={user.isDisabled ? "error" : "success"}
          >
            {user.isDisabled ? t("disabled") : t("enabled")}
          </Badge>
          {user.mustChangePassword && (
            <Badge variant="filled" size="xs" color="warning">
              {t("must_change_password")}
            </Badge>
          )}
          {user.isSuperuser && (
            <Badge variant="filled" size="xs" color="primary">
              {t("superuser")}
            </Badge>
          )}
        </Group>
      </Table.Td>
      <Table.Td>
        <Group gap="0.25rem" wrap="wrap">
          <Button
            variant="ghost"
            size="compact-xs"
            onClick={() => onResetPassword(user)}
          >
            {t("reset_password")}
          </Button>
          {!user.isSuperuser &&
            (user.isDisabled ? (
              <Button
                variant="ghost"
                size="compact-xs"
                loading={isEnablePending}
                onClick={() => onEnable(user)}
              >
                {t("enable_account")}
              </Button>
            ) : (
              <Button
                variant="ghost"
                color="error"
                size="compact-xs"
                onClick={() => onDisable(user)}
              >
                {t("disable_account")}
              </Button>
            ))}
          {!user.isSuperuser && !user.isDisabled && (
            <Button
              variant="ghost"
              color="error"
              size="compact-xs"
              onClick={() => onTransferSuperuser(user)}
            >
              {t("transfer_superuser")}
            </Button>
          )}
        </Group>
      </Table.Td>
    </Table.Tr>
  );
};

export default ManagedUserRow;
