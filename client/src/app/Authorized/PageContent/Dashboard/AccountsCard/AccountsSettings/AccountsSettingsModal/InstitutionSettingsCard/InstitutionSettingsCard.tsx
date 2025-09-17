import {
  Button,
  Card,
  Group,
  LoadingOverlay,
  Stack,
  Text,
} from "@mantine/core";
import { IInstitution } from "~/models/institution";

import React from "react";
import AccountSettingsCard from "./AccountSettingsCard/AccountSettingsCard";
import { GripVertical } from "lucide-react";
import SortableItem from "~/components/Sortable/SortableItem";
import SortableHandle from "~/components/Sortable/SortableHandle";
import Sortable from "~/components/Sortable/Sortable";
import { IAccountResponse, IAccountIndexRequest } from "~/models/account";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import { AxiosError } from "axios";
import { useDidUpdate } from "@mantine/hooks";

interface InstitutionSettingsCardProps {
  institution: IInstitution;
  isSortable: boolean;
}

const InstitutionSettingsCard = (
  props: InstitutionSettingsCardProps
): React.ReactNode => {
  const [sortedAccounts, setSortedAccounts] = React.useState<
    IAccountResponse[]
  >(
    props.institution.accounts
      .filter((a) => a.deleted === null)
      .sort((a, b) => a.index - b.index)
  );

  const { request } = React.useContext<any>(AuthContext);
  const queryClient = useQueryClient();
  const doIndexAccounts = useMutation({
    mutationFn: async (accounts: IAccountIndexRequest[]) =>
      await request({
        url: "/api/account/order",
        method: "PUT",
        data: accounts,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  useDidUpdate(() => {
    if (!props.isSortable) {
      const indexedAccounts: IAccountIndexRequest[] = sortedAccounts.map(
        (acc, index) => ({
          id: acc.id,
          index,
        })
      );
      doIndexAccounts.mutate(indexedAccounts);
    }
  }, [props.isSortable]);

  useDidUpdate(() => {
    setSortedAccounts(
      props.institution.accounts
        .filter((a) => a.deleted === null)
        .sort((a, b) => a.index - b.index)
    );
  }, [props.institution.accounts]);

  return (
    <SortableItem value={props.institution.id}>
      <Card radius="md" withBorder>
        <LoadingOverlay visible={doIndexAccounts.isPending} />
        <Group wrap="nowrap">
          {props.isSortable && (
            <SortableHandle style={{ alignSelf: "stretch" }}>
              <Button h="100%" px={0} w={30} radius="lg">
                <GripVertical size={25} />
              </Button>
            </SortableHandle>
          )}
          <Stack w="100%" gap="0.5rem">
            <Text fw={600}>{props.institution.name}</Text>
            <Stack gap={10}>
              <Sortable
                values={sortedAccounts}
                onMove={({ activeIndex: from, overIndex: to }) => {
                  const newAccounts = [...sortedAccounts];
                  const [movedAccount] = newAccounts.splice(from, 1);
                  if (movedAccount === undefined) {
                    return;
                  }
                  newAccounts.splice(to, 0, movedAccount);
                  setSortedAccounts(newAccounts);
                }}
              >
                {sortedAccounts.map((account) => (
                  <AccountSettingsCard
                    key={account.id}
                    account={account}
                    isSortable={props.isSortable}
                  />
                ))}
              </Sortable>
            </Stack>
          </Stack>
        </Group>
      </Card>
    </SortableItem>
  );
};

export default InstitutionSettingsCard;
