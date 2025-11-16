import { Accordion, ActionIcon, Modal, Stack, Text } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { SettingsIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IAccountResponse } from "~/models/account";
import DeletedAccountCard from "./DeletedAccountCard/DeletedAccountCard";

const AccountsSettings = (): React.ReactNode => {
  const [isOpened, { open, close }] = useDisclosure(false);

  const { request } = React.useContext<any>(AuthContext);

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
        p="0.5rem"
        centered
        opened={isOpened}
        onClose={close}
        title={<Text fw={600}>Accounts Settings</Text>}
        styles={{
          inner: {
            left: "0",
            right: "0",
            padding: "0 !important",
          },
        }}
      >
        <Stack gap="1rem">
          <Accordion variant="separated" multiple defaultValue={[]}>
            <Accordion.Item
              value="deleted-accounts"
              bg="var(--mantine-color-accordion-alternate)"
            >
              <Accordion.Control>
                <Text size="md" fw={600}>
                  Deleted Accounts
                </Text>
              </Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  <Text c="dimmed" size="sm" fw={600}>
                    View and restore deleted accounts.
                  </Text>
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
                    <Text c="dimmed" size="sm">
                      No deleted accounts.
                    </Text>
                  )}
                </Stack>
              </Accordion.Panel>
            </Accordion.Item>
          </Accordion>
        </Stack>
      </Modal>
    </>
  );
};

export default AccountsSettings;
