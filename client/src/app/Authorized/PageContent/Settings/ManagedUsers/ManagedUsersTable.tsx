import { Table } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import type { IManagedUserResponse } from "~/models/managedUser";
import ManagedUserRow from "./ManagedUserRow";

interface ManagedUsersTableProps {
  users: IManagedUserResponse[];
  isEnablePending: boolean;
  onResetPassword: (user: IManagedUserResponse) => void;
  onEnable: (user: IManagedUserResponse) => void;
  onDisable: (user: IManagedUserResponse) => void;
  onTransferSuperuser: (user: IManagedUserResponse) => void;
}

const ManagedUsersTable = ({
  users,
  isEnablePending,
  onResetPassword,
  onEnable,
  onDisable,
  onTransferSuperuser,
}: ManagedUsersTableProps): React.ReactNode => {
  const { t } = useTranslation();

  return (
    <Table.ScrollContainer minWidth={760}>
      <Table striped="odd" withTableBorder>
        <Table.Thead>
          <Table.Tr>
            <Table.Th>{t("managed_user")}</Table.Th>
            <Table.Th>{t("managed_user_login_methods")}</Table.Th>
            <Table.Th>{t("managed_user_status")}</Table.Th>
            <Table.Th>{t("managed_user_actions")}</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {users.map((user) => (
            <ManagedUserRow
              key={user.id}
              user={user}
              isEnablePending={isEnablePending}
              onResetPassword={onResetPassword}
              onEnable={onEnable}
              onDisable={onDisable}
              onTransferSuperuser={onTransferSuperuser}
            />
          ))}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  );
};

export default ManagedUsersTable;
