import { Accordion as MantineAccordion, Stack } from "@mantine/core";
import React from "react";
import CustomCategories from "./CustomCategories/CustomCategories";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { ITransaction } from "~/models/transaction";
import { AxiosResponse } from "axios";
import { getDeletedTransactions } from "~/helpers/transactions";
import AutomaticRules from "./AutomaticRules/AutomaticRules";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DeletedTransactionCards from "./DeletedTransactionCards/DeletedTransactionCards";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";

interface TransactionsSettingsProps {
  modalOpened: boolean;
  closeModal: () => void;
}

const TransactionsSettings = (
  props: TransactionsSettingsProps
): React.ReactNode => {
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
    (transactionsQuery.data ?? []).sort(
      (a, b) => new Date(b.date).getTime() - new Date(a.date).getTime()
    )
  );

  return (
    <Modal
      size="60rem"
      opened={props.modalOpened}
      onClose={props.closeModal}
      title={<PrimaryText size="md">{t("transactions_settings")}</PrimaryText>}
    >
      <Accordion
        defaultValue={["custom categories", "automatic rules"]}
        elevation={1}
      >
        <MantineAccordion.Item value="custom categories">
          <MantineAccordion.Control>
            <PrimaryText size="md">{t("custom_categories")}</PrimaryText>
          </MantineAccordion.Control>
          <MantineAccordion.Panel>
            <CustomCategories />
          </MantineAccordion.Panel>
        </MantineAccordion.Item>
        <MantineAccordion.Item value="automatic rules">
          <MantineAccordion.Control>
            <PrimaryText size="md">{t("automatic_rules")}</PrimaryText>
          </MantineAccordion.Control>
          <MantineAccordion.Panel>
            <AutomaticRules />
          </MantineAccordion.Panel>
        </MantineAccordion.Item>
        <MantineAccordion.Item value="deleted transactions">
          <MantineAccordion.Control>
            <PrimaryText size="md">{t("deleted_transactions")}</PrimaryText>
          </MantineAccordion.Control>
          <MantineAccordion.Panel>
            <Stack gap="0.5rem">
              <DimmedText size="sm">
                {t("view_and_restore_deleted_transactions")}
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
