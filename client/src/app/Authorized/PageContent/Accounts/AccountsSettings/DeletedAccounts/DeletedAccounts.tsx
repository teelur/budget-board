import { Group, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IAccountResponse } from "~/models/account";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DeletedAccountCard from "./DeletedAccountCard";

const DeletedAccounts = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const institutionsQuery = useQuery({
    queryKey: ["institutions"],
    queryFn: async (): Promise<any[]> => {
      const res: AxiosResponse = await request({
        url: "/api/institution",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as any[];
      }

      return [];
    },
  });

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
  });

  const deletedAccounts =
    accountsQuery.data?.filter((account) => account.deleted) ?? [];

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">
        {t("view_and_restore_deleted_accounts")}
      </DimmedText>
      {deletedAccounts.length !== 0 ? (
        deletedAccounts.map((account) => (
          <DeletedAccountCard
            key={account.id}
            account={account}
            institutionName={
              institutionsQuery.data?.find(
                (inst) => inst.id === account.institutionID,
              )?.name
            }
          />
        ))
      ) : (
        <Group justify="center">
          <DimmedText size="sm">{t("no_deleted_accounts")}</DimmedText>
        </Group>
      )}
    </Stack>
  );
};

export default DeletedAccounts;
