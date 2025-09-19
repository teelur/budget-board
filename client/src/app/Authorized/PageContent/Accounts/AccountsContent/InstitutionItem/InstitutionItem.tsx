import { Button, Card, Flex, Group, Stack, Text } from "@mantine/core";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IInstitution } from "~/models/institution";
import AccountItem from "./AccountItem/AccountItem";
import { GripVertical } from "lucide-react";
import { useSortable } from "@dnd-kit/react/sortable";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCenter } from "@dnd-kit/collision";
import { DragDropProvider } from "@dnd-kit/react";

interface IInstitutionItemProps {
  institution: IInstitution;
  userCurrency: string;
  isSortable: boolean;
  container: Element;
}

const InstitutionItem = (props: IInstitutionItemProps) => {
  const { ref, handleRef } = useSortable({
    id: props.institution.id,
    index: props.institution.index,
    modifiers: [
      RestrictToElement.configure({ element: props.container }),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCenter,
  });

  const totalBalance = props.institution.accounts
    .filter((a) => a.deleted === null)
    .reduce((acc, account) => acc + account.currentBalance, 0);
  return (
    <Card
      ref={ref}
      bg="var(--mantine-color-bg)"
      padding="0.5rem"
      radius="md"
      withBorder
    >
      <Group w="100%">
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
            <DragDropProvider>
              {props.institution.accounts
                .filter((a) => a.deleted === null)
                .map((account) => (
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
