import { Group, Stack } from "@mantine/core";
import { BodyText, Button, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { PlusIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import { Navigate } from "react-router";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
import { useManagedUsersQuery } from "~/hooks/queries/useManagedUsersQuery";
import {
  useResetManagedUserPasswordMutation,
  useSetManagedUserDisabledMutation,
  useTransferSuperuserMutation,
} from "~/hooks/mutations/useManagedUserMutations";
import type {
  IManagedUserOperationResponse,
  IManagedUserResponse,
} from "~/models/managedUser";
import { NotificationType, showNotification } from "~/helpers/notifications";
import CreateManagedUserModal from "./CreateManagedUserModal";
import ManagedUserConfirmationModal, {
  type ManagedUserConfirmation,
} from "./ManagedUserConfirmationModal";
import ManagedUsersTable from "./ManagedUsersTable";
import TemporaryPasswordModal, {
  type TemporaryPassword,
} from "./TemporaryPasswordModal";

const ManagedUsers = (): React.ReactNode => {
  const { t } = useTranslation();
  const applicationUserQuery = useApplicationUserQuery();
  const isSuperuser = applicationUserQuery.data?.isSuperuser === true;
  const managedUsersQuery = useManagedUsersQuery(isSuperuser);
  const resetPasswordMutation = useResetManagedUserPasswordMutation();
  const setDisabledMutation = useSetManagedUserDisabledMutation();
  const transferSuperuserMutation = useTransferSuperuserMutation();
  const [isCreateModalOpen, setIsCreateModalOpen] = React.useState(false);
  const [confirmation, setConfirmation] =
    React.useState<ManagedUserConfirmation | null>(null);
  const [temporaryPassword, setTemporaryPassword] =
    React.useState<TemporaryPassword | null>(null);

  const handleTemporaryPassword = (response: IManagedUserOperationResponse) => {
    if (!response.temporaryPassword) {
      showNotification({
        type: NotificationType.Error,
        message: t("temporary_password_unavailable"),
      });
      return;
    }

    setTemporaryPassword({
      username: response.user.username,
      password: response.temporaryPassword,
    });
  };

  const confirmAction = () => {
    if (!confirmation) {
      return;
    }

    const { action, user } = confirmation;
    setConfirmation(null);

    if (action === "reset_password") {
      resetPasswordMutation.mutate(user.id, {
        onSuccess: (response) => {
          setConfirmation(null);
          handleTemporaryPassword(response);
        },
      });
    } else if (action === "disable") {
      setDisabledMutation.mutate(
        { userId: user.id, disabled: true },
        { onSuccess: () => setConfirmation(null) },
      );
    } else {
      transferSuperuserMutation.mutate(user.id, {
        onSuccess: () => setConfirmation(null),
      });
    }
  };

  if (applicationUserQuery.isPending) {
    return <Skeleton height={300} />;
  }

  if (applicationUserQuery.isError) {
    return <BodyText>{t("managed_users_access_check_error")}</BodyText>;
  }

  if (!isSuperuser) {
    return <Navigate to="/settings/user" replace />;
  }

  if (managedUsersQuery.isError) {
    return <BodyText>{t("managed_users_query_error")}</BodyText>;
  }

  return (
    <Stack gap="0.75rem">
      <Group justify="space-between" align="flex-start">
        <BodyText tone="secondary" size="sm">
          {t("managed_users_description")}
        </BodyText>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          leftSection={<PlusIcon size="1rem" />}
          onClick={() => setIsCreateModalOpen(true)}
        >
          {t("create_managed_user")}
        </Button>
      </Group>

      {managedUsersQuery.isPending ? (
        <Skeleton height={220} />
      ) : managedUsersQuery.data?.length ? (
        <ManagedUsersTable
          users={managedUsersQuery.data}
          isEnablePending={setDisabledMutation.isPending}
          onResetPassword={(user: IManagedUserResponse) =>
            setConfirmation({ action: "reset_password", user })
          }
          onEnable={(user: IManagedUserResponse) =>
            setDisabledMutation.mutate({ userId: user.id, disabled: false })
          }
          onDisable={(user: IManagedUserResponse) =>
            setConfirmation({ action: "disable", user })
          }
          onTransferSuperuser={(user: IManagedUserResponse) =>
            setConfirmation({ action: "transfer_superuser", user })
          }
        />
      ) : (
        <BodyText>{t("no_managed_users")}</BodyText>
      )}

      <CreateManagedUserModal
        opened={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onTemporaryPassword={handleTemporaryPassword}
      />
      <ManagedUserConfirmationModal
        confirmation={confirmation}
        isPending={
          resetPasswordMutation.isPending ||
          setDisabledMutation.isPending ||
          transferSuperuserMutation.isPending
        }
        onClose={() => setConfirmation(null)}
        onConfirm={confirmAction}
      />
      <TemporaryPasswordModal
        temporaryPassword={temporaryPassword}
        onClose={() => setTemporaryPassword(null)}
      />
    </Stack>
  );
};

export default ManagedUsers;
