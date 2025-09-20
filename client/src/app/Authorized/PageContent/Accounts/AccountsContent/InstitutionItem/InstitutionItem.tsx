import {
  Button,
  Card,
  Flex,
  Group,
  LoadingOverlay,
  Stack,
  Text,
} from "@mantine/core";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IInstitution } from "~/models/institution";
import AccountItem from "./AccountItem/AccountItem";
import { GripVertical } from "lucide-react";
import { useSortable } from "@dnd-kit/react/sortable";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCorners } from "@dnd-kit/collision";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { IAccountIndexRequest, IAccountResponse } from "~/models/account";
import React from "react";
import { useDidUpdate } from "@mantine/hooks";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";

interface IInstitutionItemProps {
  institution: IInstitution;
  userCurrency: string;
  isSortable: boolean;
  container: Element;
}

const InstitutionItem = (props: IInstitutionItemProps) => {
  const [sortedAccounts, setSortedAccounts] = React.useState<
    IAccountResponse[]
  >(props.institution.accounts.sort((a, b) => a.index - b.index));

  const { ref, handleRef } = useSortable({
    id: props.institution.id,
    index: props.institution.index,
    modifiers: [
      RestrictToElement.configure({ element: props.container }),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCorners,
  });

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
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  const totalBalance = props.institution.accounts
    .filter((a) => a.deleted === null)
    .reduce((acc, account) => acc + account.currentBalance, 0);

  useDidUpdate(() => {
    setSortedAccounts(
      props.institution.accounts.sort((a, b) => a.index - b.index)
    );
  }, [props.institution.accounts]);

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

  return (
    <Card
      ref={ref}
      bg="var(--mantine-color-bg)"
      padding="0.5rem"
      radius="md"
      withBorder
    >
      <LoadingOverlay visible={doIndexAccounts.isPending} />
      <Group w="100%" wrap="nowrap" gap="0.5rem" align="flex-start">
        {props.isSortable && (
          <Flex ref={handleRef} style={{ alignSelf: "stretch" }}>
            <Button h="100%" px={0} w={30} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        <Stack gap="0.5rem" flex="1 1 auto">
          <Group justify="space-between" align="center">
            <Text fw={600} size="md">
              {props.institution.name}
            </Text>
            <Text fw={600} size="md" c={totalBalance < 0 ? "red" : "green"}>
              {convertNumberToCurrency(totalBalance, true, props.userCurrency)}
            </Text>
          </Group>
          <Stack id={props.institution.id} gap="0.5rem">
            <DragDropProvider
              onDragEnd={(event) => {
                const updatedList = move(sortedAccounts, event).map(
                  (acc, index) => ({
                    ...acc,
                    index,
                  })
                );
                setSortedAccounts(updatedList);
              }}
            >
              {sortedAccounts.map((account) => (
                <AccountItem
                  key={account.id}
                  account={account}
                  userCurrency={props.userCurrency}
                  isSortable={props.isSortable}
                  container={
                    document.getElementById(props.institution.id) as Element
                  }
                />
              ))}
            </DragDropProvider>
          </Stack>
        </Stack>
      </Group>
    </Card>
  );
};

export default InstitutionItem;
