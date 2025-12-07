import { ActionIcon, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { SettingsIcon } from "lucide-react";
import Modal from "~/components/core/Modal/Modal";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import {
  isNetWorthWidgetType,
  parseNetWorthConfiguration,
} from "~/helpers/widgets";
import { IAccountResponse } from "~/models/account";
import { IAssetResponse } from "~/models/asset";
import { IWidgetSettingsResponse } from "~/models/widgetSettings";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import NetWorthLineItem from "./NetWorthLineItem/NetWorthLineItem";

const NetWorthCardSettings = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const { request } = useAuth();
  const widgetSettingsQuery = useQuery({
    queryKey: ["widgetSettings"],
    queryFn: async (): Promise<IWidgetSettingsResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/widgetSettings",
        method: "GET",
      });
      if (res.status === 200) {
        return res.data as IWidgetSettingsResponse[];
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

  const assetsQuery = useQuery({
    queryKey: ["assets"],
    queryFn: async (): Promise<IAssetResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/asset",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAssetResponse[];
      }

      return [];
    },
  });

  const lines = (() => {
    const netWorthWidget = widgetSettingsQuery.data?.find((ws) =>
      isNetWorthWidgetType(ws.widgetType)
    );
    if (!netWorthWidget) {
      return undefined;
    }

    return parseNetWorthConfiguration(netWorthWidget.configuration)?.lines;
  })();

  return (
    <>
      <ActionIcon
        variant="subtle"
        size="md"
        c="var(--base-color-text-dimmed)"
        onClick={open}
      >
        <SettingsIcon />
      </ActionIcon>
      <Modal
        opened={opened}
        onClose={close}
        title={<PrimaryText size="md">Net Worth Settings</PrimaryText>}
      >
        <Stack>
          <DimmedText size="sm">
            Configure the data that appears in the Net Worth widget.
          </DimmedText>
          <Stack gap="0.25rem">
            {lines && lines.length > 0 ? (
              lines.map((line) => (
                <NetWorthLineItem
                  key={`${line.group}-${line.index}-${line.name}`}
                  line={line}
                />
              ))
            ) : (
              <DimmedText size="sm">No lines available.</DimmedText>
            )}
          </Stack>
        </Stack>
      </Modal>
    </>
  );
};

export default NetWorthCardSettings;
