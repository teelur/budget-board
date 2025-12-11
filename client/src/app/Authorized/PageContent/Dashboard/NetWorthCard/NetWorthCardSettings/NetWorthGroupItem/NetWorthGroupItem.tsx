import Card from "~/components/core/Card/Card";
import { INetWorthWidgetLine } from "~/models/widgetSettings";
import NetWorthLineItem from "../NetWorthLineItem/NetWorthLineItem";
import { ActionIcon, Group, Stack } from "@mantine/core";
import { PlusIcon } from "lucide-react";
import { useNetWorthSettings } from "~/providers/NetWorthSettingsProvider/NetWorthSettingsProvider";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { INetWorthWidgetLineCreateRequest } from "~/models/netWorthWidgetConfiguration";

export interface NetWorthGroupItemProps {
  lines: INetWorthWidgetLine[];
  lineIndexOffset: number;
  updateNetWorthLine: (updatedLine: INetWorthWidgetLine, index: number) => void;
}

const NetWorthGroupItem = (props: NetWorthGroupItemProps): React.ReactNode => {
  const { settingsId } = useNetWorthSettings();
  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doCreateLine = useMutation({
    mutationFn: async (newLine: INetWorthWidgetLineCreateRequest) =>
      await request({
        url: `/api/netWorthWidgetLine`,
        method: "POST",
        data: newLine,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["widgetSettings"] });

      notifications.show({
        color: "var(--button-color-confirm)",
        message: "Net worth settings updated successfully.",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <Card elevation={0}>
      <Stack gap="0.5rem">
        <Group justify="flex-end">
          <ActionIcon
            size="sm"
            loading={doCreateLine.isPending}
            onClick={async () =>
              await doCreateLine.mutateAsync({
                name: "",
                group: props.lines[0]?.group ?? 0,
                index: props.lines.length,
                widgetSettingsId: settingsId,
              } as INetWorthWidgetLineCreateRequest)
            }
          >
            <PlusIcon />
          </ActionIcon>
        </Group>
        <Stack gap="0.25rem">
          {props.lines.map((line, index) => (
            <NetWorthLineItem
              key={line.id}
              line={line}
              index={index}
              updateNetWorthLine={(updatedLine, lineIndex) => {
                props.updateNetWorthLine(
                  updatedLine,
                  props.lineIndexOffset + lineIndex
                );
              }}
            />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default NetWorthGroupItem;
