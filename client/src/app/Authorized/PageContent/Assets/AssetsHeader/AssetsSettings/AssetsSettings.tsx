import { Accordion, ActionIcon, Modal, Stack, Text } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { SettingsIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import DeletedAssetCard from "./DeletedAssetCard/DeletedAssetCard";

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
        size="40rem"
        p="0.5rem"
        centered
        opened={isOpened}
        onClose={close}
        title={<Text fw={600}>Assets Settings</Text>}
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
              value="deleted-assets"
              bg="var(--mantine-color-accordion-alternate)"
            >
              <Accordion.Control>
                <Text size="md" fw={600}>
                  Deleted Assets
                </Text>
              </Accordion.Control>
              <Accordion.Panel>
                <Stack gap="0.5rem">
                  <Text c="dimmed" size="sm" fw={600}>
                    View and restore deleted assets.
                  </Text>
                  {deletedAssets.length !== 0 ? (
                    deletedAssets.map((asset) => (
                      <DeletedAssetCard key={asset.id} asset={asset} />
                    ))
                  ) : (
                    <Text c="dimmed" size="sm">
                      No deleted assets.
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

export default AssetsSettings;
