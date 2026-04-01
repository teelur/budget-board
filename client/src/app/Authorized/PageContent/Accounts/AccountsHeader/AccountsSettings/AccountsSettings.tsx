import {
  Accordion as MantineAccordion,
  ActionIcon,
  Group,
  Stack,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { SettingsIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IAccountResponse } from "~/models/account";
import DeletedAccountCard from "./DeletedAccountCard/DeletedAccountCard";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";
import { useTranslation } from "react-i18next";

const AccountsSettings = (): React.ReactNode => {
  const [isOpened, { open, close }] = useDisclosure(false);

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
    <>
      <ActionIcon variant="subtle" size="input-sm" onClick={open}>
        <SettingsIcon />
      </ActionIcon>
      <Modal
        size="40rem"
        opened={isOpened}
        onClose={close}
        title={<PrimaryText size="md">{t("accounts_settings")}</PrimaryText>}
      >
        <Stack gap="1rem">
          <Accordion elevation={1}>
            <MantineAccordion.Item value="deleted-accounts">
              <MantineAccordion.Control>
                <PrimaryText size="md">{t("deleted_accounts")}</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
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
                            (inst) => inst.id === account.institutionID
                          )?.name
                        }
                      />
                    ))
                  ) : (
                    <Group justify="center">
                      <DimmedText size="sm">
                        {t("no_deleted_accounts")}
                      </DimmedText>
                    </Group>
                  )}
                </Stack>
              </MantineAccordion.Panel>
            </MantineAccordion.Item>
          </Accordion>
        </Stack>
      </Modal>
    </>
  );
};

export default AccountsSettings;
