import { Accordion, Stack } from "@mantine/core";
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
import SurfaceAccordionRoot from "~/components/Accordion/Surface/SurfaceAccordionRoot/SurfaceAccordionRoot";
import DimmedText from "~/components/Text/DimmedText/DimmedText";
import DeletedTransactionCards from "./DeletedTransactionCards/DeletedTransactionCards";

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
      <SurfaceAccordionRoot
        defaultValue={["custom categories", "automatic rule"]}
      >
        <Accordion.Item value="custom categories">
          <Accordion.Control>
            <PrimaryText size="md">Custom Categories</PrimaryText>
          </Accordion.Control>
          <Accordion.Panel>
            <CustomCategories />
          </Accordion.Panel>
        </Accordion.Item>
        <Accordion.Item value="automatic rule">
          <Accordion.Control>
            <PrimaryText size="md">Automatic Rules</PrimaryText>
          </Accordion.Control>
          <Accordion.Panel>
            <AutomaticRules />
          </Accordion.Panel>
        </Accordion.Item>
        <Accordion.Item value="deleted transactions">
          <Accordion.Control>
            <PrimaryText size="md">Deleted Transactions</PrimaryText>
          </Accordion.Control>
          <Accordion.Panel>
            <Stack gap="0.5rem">
              <DimmedText size="sm">
                View and restore deleted transactions.
              </DimmedText>
              <DeletedTransactionCards transactions={deletedTransactions} />
            </Stack>
          </Accordion.Panel>
        </Accordion.Item>
      </SurfaceAccordionRoot>
    </Modal>
  );
};

export default TransactionsSettings;
