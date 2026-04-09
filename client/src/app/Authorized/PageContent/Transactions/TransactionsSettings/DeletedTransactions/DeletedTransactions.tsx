import { Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ITransaction } from "~/models/transaction";
import { AxiosResponse } from "axios";
import { getDeletedTransactions } from "~/helpers/transactions";
import DeletedTransactionCards from "./DeletedTransactionCards/DeletedTransactionCards";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";

const Deleted = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const transactionsQuery = useQuery({
    queryKey: ["transactions", { getHidden: true }],
    queryFn: async (): Promise<ITransaction[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transaction",
        method: "GET",
        params: { getHidden: true },
      });

      if (res.status === 200) {
        return res.data as ITransaction[];
      }

      return [];
    },
  });

  const deletedTransactions = getDeletedTransactions(
    [...(transactionsQuery.data ?? [])].sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime(),
    ),
  );

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">
        {t("view_and_restore_deleted_transactions")}
      </DimmedText>
      <DeletedTransactionCards transactions={deletedTransactions} />
    </Stack>
  );
};

export default Deleted;
