import { Accordion, ActionIcon, Group, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { SettingsIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IAccountResponse } from "~/models/account";
import DeletedAccountCard from "./DeletedAccountCard/DeletedAccountCard";
import Modal from "~/components/Modal/Modal";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import SurfaceAccordionRoot from "~/components/Accordion/Surface/SurfaceAccordionRoot/SurfaceAccordionRoot";
import DimmedText from "~/components/Text/DimmedText/DimmedText";

const AccountsSettings = (): React.ReactNode => {
  const [isOpened, { open, close }] = useDisclosure(false);

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
        title={<PrimaryText size="md">Accounts Settings</PrimaryText>}
      >
        <Stack gap="1rem">
          <SurfaceAccordionRoot defaultValue={[]}>
            <Accordion.Item
              value="deleted-accounts"
              bg="var(--mantine-color-accordion-alternate)"
            >
              <Accordion.Control>
                <PrimaryText size="md">Deleted Accounts</PrimaryText>
              </Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  <DimmedText size="sm">
                    View and restore deleted accounts.
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
                      <DimmedText size="sm">No deleted accounts.</DimmedText>
                    </Group>
                  )}
                </Stack>
              </Accordion.Panel>
            </Accordion.Item>
          </SurfaceAccordionRoot>
        </Stack>
      </Modal>
    </>
  );
};

export default AccountsSettings;
