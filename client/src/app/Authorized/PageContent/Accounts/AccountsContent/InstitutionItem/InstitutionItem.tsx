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
import InstitutionItemContent from "./InstitutionItemContent/InstitutionItemContent";
import EditableInstitutionItemContent from "./EditableInstitutionItemContent/EditableInstitutionItemContent";
import Card from "~/components/core/Card/Card";
import { useOrderAccountsMutation } from "~/hooks/mutations/accounts/useOrderAccountsMutation";
import Divider from "~/components/core/Divider/Divider";

interface IInstitutionItemProps {
  institution: IInstitution;
  isSortable: boolean;
  container?: Element;
  openDetails: (account: IAccountResponse | undefined) => void;
}

const InstitutionItem = (props: IInstitutionItemProps) => {
  const [isEditable, { toggle }] = useDisclosure(false);
  const [accountsContainer, setAccountsContainer] =
    React.useState<HTMLDivElement | null>(null);

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
      })),
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
        })),
    );
  }, [props.institution.accounts]);

  const { ref, handleRef } = useSortable({
    id: props.institution.id,
    index: props.institution.index,
    modifiers: [
      ...(props.container
        ? [RestrictToElement.configure({ element: props.container })]
        : []),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCorners,
  });

  const orderAccountsMutation = useOrderAccountsMutation();

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
        })),
    );
  }, [props.institution.accounts]);

  useDidUpdate(() => {
    if (!props.isSortable) {
      const indexedAccounts: IAccountIndexRequest[] = sortedAccounts.map(
        (acc, index) => ({
          id: acc.id,
          index,
        }),
      );
      orderAccountsMutation.mutate(indexedAccounts);
    }
  }, [props.isSortable]);

  return (
    <Card
      p={0}
      style={{
        borderWidth: "2px",
        display: "flex",
        flexDirection: "column",
      }}
      ref={props.isSortable ? ref : undefined}
      elevation={1}
    >
      <LoadingOverlay visible={orderAccountsMutation.isPending} />
      <Stack px="0.5rem" py="0.25rem">
        {isEditable ? (
          <EditableInstitutionItemContent
            institution={props.institution}
            totalBalance={totalBalance}
            toggle={toggle}
          />
        ) : (
          <InstitutionItemContent
            institution={props.institution}
            totalBalance={totalBalance}
            toggle={toggle}
          />
        )}
      </Stack>
      <Divider w="100%" size="sm" elevation={0} />
      <Group w="100%" p={0} wrap="nowrap" gap="0.25rem" align="flex-start">
        {props.isSortable && (
          <Flex m="0.25rem" ref={handleRef} style={{ alignSelf: "stretch" }}>
            <Button h="100%" px={0} w={30} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        <Stack
          ref={setAccountsContainer}
          id={props.institution.id}
          w="100%"
          py="0.125rem"
          gap="0.5rem"
        >
          <DragDropProvider
            onDragEnd={(event) => {
              const updatedList = move(sortedAccounts, event).map(
                (acc, index) => ({
                  ...acc,
                  index,
                }),
              );

              setSortedAccounts(updatedList);
            }}
          >
            {sortedAccounts.map((account, index) => (
              <React.Fragment key={account.id}>
                <AccountItem
                  account={account}
                  isSortable={props.isSortable}
                  container={accountsContainer ?? undefined}
                  openDetails={props.openDetails}
                />
                {index < sortedAccounts.length - 1 && (
                  <Divider w="100%" p={0} size="xs" elevation={0} />
                )}
              </React.Fragment>
            ))}
          </DragDropProvider>
        </Stack>
      </Group>
    </Card>
  );
};

export default InstitutionItem;
