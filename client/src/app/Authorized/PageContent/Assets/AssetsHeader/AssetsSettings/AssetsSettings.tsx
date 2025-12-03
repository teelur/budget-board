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
import DeletedAssetCard from "./DeletedAssetCard/DeletedAssetCard";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import Accordion from "~/components/core/Accordion/Accordion";

const AssetsSettings = (): React.ReactNode => {
  const [isOpened, { open, close }] = useDisclosure(false);

  const { request } = useAuth();

  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async (): Promise<any[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as any[];
      }

      return [];
    },
  });

  const deletedAssets =
    assetsQuery.data?.filter((asset) => asset.deleted) ?? [];

  return (
    <>
      <ActionIcon variant="subtle" size="input-sm" onClick={open}>
        <SettingsIcon />
      </ActionIcon>
      <Modal
        opened={isOpened}
        onClose={close}
        title={<PrimaryText>Assets Settings</PrimaryText>}
        size="40rem"
      >
        <Stack gap="1rem">
          <Accordion defaultValue={[]} elevation={1}>
            <MantineAccordion.Item value="deleted-assets">
              <MantineAccordion.Control>
                <PrimaryText size="md">Deleted Assets</PrimaryText>
              </MantineAccordion.Control>
              <MantineAccordion.Panel>
                <Stack gap="0.5rem">
                  <DimmedText size="sm">
                    View and restore deleted assets.
                  </DimmedText>
                  {deletedAssets.length !== 0 ? (
                    deletedAssets.map((asset) => (
                      <DeletedAssetCard key={asset.id} asset={asset} />
                    ))
                  ) : (
                    <Group justify="center">
                      <DimmedText size="xs">No deleted assets.</DimmedText>
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

export default AssetsSettings;
