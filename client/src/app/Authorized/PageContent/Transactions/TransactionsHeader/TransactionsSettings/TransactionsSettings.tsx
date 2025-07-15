import { Accordion, Modal, Stack } from "@mantine/core";
import React from "react";
import CustomCategories from "./CustomCategories/CustomCategories";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ITransaction } from "~/models/transaction";
import { AxiosResponse } from "axios";
import { getDeletedTransactions } from "~/helpers/transactions";
import DeletedTransactionsCard from "./DeletedTransactionCard/DeletedTransactionsCard";

interface TransactionsSettingsProps {
  modalOpened: boolean;
  closeModal: () => void;
}

const TransactionsSettings = (
  props: TransactionsSettingsProps
): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

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
    (transactionsQuery.data ?? []).sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
    )
  );

  return (
    <Modal
      size="40rem"
      centered
      padding="0.5rem"
      opened={props.modalOpened}
      onClose={props.closeModal}
      title="Transactions Settings"
      styles={{
        inner: {
          left: "0",
          right: "0",
          padding: "0 !important",
        },
      }}
    >
      <Accordion
        variant="separated"
        multiple
        defaultValue={["custom categories"]}
      >
        <Accordion.Item value="custom categories">
          <Accordion.Control>Custom Categories</Accordion.Control>
          <Accordion.Panel>
            <CustomCategories />
          </Accordion.Panel>
        </Accordion.Item>
        <Accordion.Item value="deleted transactions">
          <Accordion.Control>Deleted Transactions</Accordion.Control>
          <Accordion.Panel>
            <Stack gap="0.5rem">
              {deletedTransactions.length !== 0 ? (
                deletedTransactions.map((deletedTransaction: ITransaction) => (
                  <DeletedTransactionsCard
                    key={deletedTransaction.id}
                    deletedTransaction={deletedTransaction}
                  />
                ))
              ) : (
                <span>No deleted transactions.</span>
              )}
            </Stack>
          </Accordion.Panel>
        </Accordion.Item>
      </Accordion>
    </Modal>
  );
};

export default TransactionsSettings;
