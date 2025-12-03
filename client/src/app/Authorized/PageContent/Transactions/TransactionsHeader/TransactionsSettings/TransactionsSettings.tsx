import { Accordion as MantineAccordion, Stack } from "@mantine/core";
import React from "react";
import CustomCategories from "./CustomCategories/CustomCategories";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ITransaction } from "~/models/transaction";
import { AxiosResponse } from "axios";
import { getDeletedTransactions } from "~/helpers/transactions";
import AutomaticRules from "./AutomaticRules/AutomaticRules";
import Modal from "~/components/Modal/Modal";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/Text/DimmedText/DimmedText";
import DeletedTransactionCards from "./DeletedTransactionCards/DeletedTransactionCards";
import Accordion from "~/components/Accordion/Accordion";

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
    <Modal
      size="60rem"
      opened={props.modalOpened}
      onClose={props.closeModal}
      title={<PrimaryText size="md">Transactions Settings</PrimaryText>}
    >
      <Accordion
        defaultValue={["custom categories", "automatic rule"]}
        elevation={1}
      >
        <MantineAccordion.Item value="custom categories">
          <MantineAccordion.Control>
            <PrimaryText size="md">Custom Categories</PrimaryText>
          </MantineAccordion.Control>
          <MantineAccordion.Panel>
            <CustomCategories />
          </MantineAccordion.Panel>
        </MantineAccordion.Item>
        <MantineAccordion.Item value="automatic rule">
          <MantineAccordion.Control>
            <PrimaryText size="md">Automatic Rules</PrimaryText>
          </MantineAccordion.Control>
          <MantineAccordion.Panel>
            <AutomaticRules />
          </MantineAccordion.Panel>
        </MantineAccordion.Item>
        <MantineAccordion.Item value="deleted transactions">
          <MantineAccordion.Control>
            <PrimaryText size="md">Deleted Transactions</PrimaryText>
          </MantineAccordion.Control>
          <MantineAccordion.Panel>
            <Stack gap="0.5rem">
              <DimmedText size="sm">
                View and restore deleted transactions.
              </DimmedText>
              <DeletedTransactionCards transactions={deletedTransactions} />
            </Stack>
          </MantineAccordion.Panel>
        </MantineAccordion.Item>
      </Accordion>
    </Modal>
  );
};

export default TransactionsSettings;
