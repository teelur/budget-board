import { Button, Flex, Group, LoadingOverlay, Stack } from "@mantine/core";
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
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import InstitutionItemContent from "./InstitutionItemContent/InstitutionItemContent";
import EditableInstitutionItemContent from "./EditableInstitutionItemContent/EditableInstitutionItemContent";
import Card from "~/components/core/Card/Card";

interface IInstitutionItemProps {
  institution: IInstitution;
  userCurrency: string;
  isSortable: boolean;
  container: Element;
  openDetails: (account: IAccountResponse | undefined) => void;
}

const InstitutionItem = (props: IInstitutionItemProps) => {
  const [isSelected, { toggle }] = useDisclosure(false);

  // Some accounts might have conflicting indices, so we need to re-index them here
  // to ensure the drag-and-drop functionality works correctly
  const [sortedAccounts, setSortedAccounts] = React.useState<
    IAccountResponse[]
  >(
    props.institution.accounts
      .slice()
      .filter((a) => a.deleted === null)
      .sort((a, b) => a.index - b.index)
      .map((a, index) => ({
        ...a,
        index,
      }))
  );

  React.useEffect(() => {
    setSortedAccounts(
      props.institution.accounts
        .slice()
        .filter((a) => a.deleted === null)
        .sort((a, b) => a.index - b.index)
        .map((a, index) => ({
          ...a,
          index,
        }))
    );
  }, [props.institution.accounts]);

  const { ref, handleRef } = useSortable({
    id: props.institution.id,
    index: props.institution.index,
    modifiers: [
      RestrictToElement.configure({ element: props.container }),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCorners,
  });

  const { request } = useAuth();
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
      props.institution.accounts
        .slice()
        .filter((a) => a.deleted === null)
        .sort((a, b) => a.index - b.index)
        .map((a, index) => ({
          ...a,
          index,
        }))
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
    <Card ref={props.isSortable ? ref : undefined} elevation={1}>
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
          {isSelected ? (
            <EditableInstitutionItemContent
              institution={props.institution}
              totalBalance={totalBalance}
              userCurrency={props.userCurrency}
              toggle={toggle}
            />
          ) : (
            <InstitutionItemContent
              institution={props.institution}
              totalBalance={totalBalance}
              userCurrency={props.userCurrency}
              toggle={toggle}
            />
          )}

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
                  openDetails={props.openDetails}
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
