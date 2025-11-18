import { Accordion, Modal, Stack, Text } from "@mantine/core";
import React from "react";
import CustomCategories from "./CustomCategories/CustomCategories";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ITransaction } from "~/models/transaction";
import { AxiosResponse } from "axios";
import { getDeletedTransactions } from "~/helpers/transactions";
import DeletedTransactionsCard from "./DeletedTransactionCard/DeletedTransactionsCard";
import AutomaticRules from "./AutomaticRules/AutomaticRules";

interface TransactionsSettingsProps {
  modalOpened: boolean;
  closeModal: () => void;
}

const TransactionsSettings = (
  props: TransactionsSettingsProps
): React.ReactNode => {
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
    (transactionsQuery.data ?? []).sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
    )
  );

  return (
    <Modal.Root
      size="40rem"
      centered
      padding="0.5rem"
      opened={props.modalOpened}
      onClose={props.closeModal}
      styles={{
        inner: {
          left: "0",
          right: "0",
          padding: "0 !important",
        },
      }}
    >
      <Modal.Overlay />
      <Modal.Content bg="var(--mantine-color-content-background)">
        <Modal.Header bg="var(--mantine-color-content-background)">
          <Modal.Title>
            <Text fw={600}>Transactions Settings</Text>
          </Modal.Title>
          <Modal.CloseButton />
        </Modal.Header>
        <Modal.Body>
          <Accordion
            variant="separated"
            multiple
            defaultValue={["custom categories", "automatic rule"]}
          >
            <Accordion.Item
              value="custom categories"
              bg="var(--mantine-color-accordion-alternate)"
            >
              <Accordion.Control>
                <Text size="md" fw={600}>
                  Custom Categories
                </Text>
              </Accordion.Control>
              <Accordion.Panel>
                <CustomCategories />
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item
              value="automatic rule"
              bg="var(--mantine-color-accordion-alternate)"
            >
              <Accordion.Control>
                <Text size="md" fw={600}>
                  Automatic Rules
                </Text>
              </Accordion.Control>
              <Accordion.Panel>
                <AutomaticRules />
              </Accordion.Panel>
            </Accordion.Item>
            <Accordion.Item
              value="deleted transactions"
              bg="var(--mantine-color-accordion-alternate)"
            >
              <Accordion.Control>
                <Text size="md" fw={600}>
                  Deleted Transactions
                </Text>
              </Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  <Text c="dimmed" size="sm" fw={600}>
                    View and restore deleted transactions.
                  </Text>
                  {deletedTransactions.length !== 0 ? (
                    deletedTransactions.map(
                      (deletedTransaction: ITransaction) => (
                        <DeletedTransactionsCard
                          key={deletedTransaction.id}
                          deletedTransaction={deletedTransaction}
                        />
                      )
                    )
                  ) : (
                    <span>No deleted transactions.</span>
                  )}
                </Stack>
              </Accordion.Panel>
            </Accordion.Item>
          </Accordion>
        </Modal.Body>
      </Modal.Content>
    </Modal.Root>
  );
};

export default TransactionsSettings;
