import {
  ActionIcon,
  Flex,
  Group,
  Modal,
  NumberInput,
  Text,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { SendIcon, SettingsIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  IUserSettings,
  IUserSettingsUpdateRequest,
} from "~/models/userSettings";

const BudgetSettings = (): React.ReactNode => {
  const [settingsOpen, { open, close }] = useDisclosure(false);

  const { request } = React.useContext<any>(AuthContext);

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const budgetWarningThresholdField = useField<number>({
    initialValue: userSettingsQuery.data?.budgetWarningThreshold ?? 80,
    validate: (value) =>
      value < 0 || value > 100
        ? "Warning threshold must be between 0 and 100"
        : null,
  });

  const queryClient = useQueryClient();
  const doUpdateUserSettings = useMutation({
    mutationFn: async (updatedUserSettings: IUserSettingsUpdateRequest) =>
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: updatedUserSettings,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["userSettings"] });
      notifications.show({ message: "Settings updated", color: "green" });
    },
    onError: (error: any) => {
      notifications.show({
        color: "red",
        message: translateAxiosError(error),
      });
    },
  });

  React.useEffect(() => {
    if (userSettingsQuery.data) {
      budgetWarningThresholdField.setValue(
        userSettingsQuery.data.budgetWarningThreshold
      );
    }
  }, [userSettingsQuery.data]);
  return (
    <>
      <ActionIcon
        variant="subtle"
        size="input-sm"
        onClick={open}
        aria-label="Open budget settings"
      >
        <SettingsIcon />
      </ActionIcon>
      <Modal
        opened={settingsOpen}
        onClose={close}
        title={<Text fw={600}>Budget Settings</Text>}
        styles={{
          inner: {
            left: "0",
            right: "0",
            padding: "0 !important",
          },
        }}
      >
        <Group gap="0.5rem" wrap="nowrap">
          <NumberInput
            flex="1 1 auto"
            label="Budget Warning Threshold"
            description="Set the percentage threshold at which budgets will turn yellow."
            min={0}
            max={100}
            suffix="%"
            {...budgetWarningThresholdField.getInputProps()}
          />
          <Flex style={{ alignSelf: "stretch" }} p={0}>
            <ActionIcon
              h="100%"
              size="md"
              aria-label="Save settings"
              onClick={() => {
                if (budgetWarningThresholdField.error) {
                  notifications.show({
                    color: "red",
                    message: budgetWarningThresholdField.error,
                  });
                  return;
                }

                doUpdateUserSettings.mutate({
                  budgetWarningThreshold:
                    budgetWarningThresholdField.getValue(),
                });
              }}
              loading={doUpdateUserSettings.isPending}
            >
              <SendIcon size={20} />
            </ActionIcon>
          </Flex>
        </Group>
      </Modal>
    </>
  );
};

export default BudgetSettings;
