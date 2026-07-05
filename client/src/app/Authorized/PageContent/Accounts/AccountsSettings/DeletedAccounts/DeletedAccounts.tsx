import { Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DeletedAccountCard from "./DeletedAccountCard";
import { useInstitutionsQuery } from "~/hooks/queries/useInstitutionsQuery";

const DeletedAccounts = (): React.ReactNode => {
  const { t } = useTranslation();
  const institutionsQuery = useInstitutionsQuery();

  const getDeletedAccountsContent = (): React.ReactNode => {
    if (institutionsQuery.isPending) {
      return <Skeleton height={55} radius="md" />;
    }

    const deletedAccounts = (institutionsQuery.data ?? [])
      .map((institution) => institution.accounts ?? [])
      .flat()
      .filter((account) => account.deleted);

    if (deletedAccounts.length === 0) {
      return (
        <Group justify="center">
          <DimmedText size="sm">{t("no_deleted_accounts")}</DimmedText>
        </Group>
      );
    }

    return deletedAccounts.map((account) => (
      <DeletedAccountCard
        key={account.id}
        account={account}
        institutionName={
          institutionsQuery.data?.find(
            (inst) => inst.id === account.institutionID,
          )?.name
        }
      />
    ));
  };

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">
        {t("view_and_restore_deleted_accounts")}
      </DimmedText>
      {getDeletedAccountsContent()}
    </Stack>
  );
};

export default DeletedAccounts;
