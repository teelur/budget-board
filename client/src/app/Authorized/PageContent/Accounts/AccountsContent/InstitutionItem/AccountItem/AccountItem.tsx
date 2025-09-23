import classes from "./AccountItem.module.css";

import { useSortable } from "@dnd-kit/react/sortable";
import { Button, Card, Flex, Group } from "@mantine/core";
import { GripVertical } from "lucide-react";
import { IAccountResponse } from "~/models/account";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCenter } from "@dnd-kit/collision";
import { useDisclosure } from "@mantine/hooks";
import AccountItemContent from "./AccountItemContent/AccountItemContent";
import EditableAccountItemContent from "./EditableAccountItemContent/EditableAccountItemContent";

interface IAccountItemProps {
  account: IAccountResponse;
  userCurrency: string;
  isSortable: boolean;
  container: Element;
  openDetails: (account: IAccountResponse | undefined) => void;
}

const AccountItem = (props: IAccountItemProps) => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const { ref, handleRef } = useSortable({
    id: props.account.id,
    index: props.account.index,
    modifiers: [
      RestrictToElement.configure({
        element: props.container,
      }),
      RestrictToVerticalAxis,
    ],
    collisionDetector: closestCenter,
  });

  return (
    <Card
      ref={props.isSortable ? ref : undefined}
      shadow="sm"
      padding="0.5rem"
      radius="md"
      className={classes.card}
      withBorder
      onClick={() => props.openDetails(props.account)}
    >
      <Group w="100%" gap="0.5rem" wrap="nowrap">
        {props.isSortable && (
          <Flex style={{ alignSelf: "stretch" }}>
            <Button ref={handleRef} h="100%" px={0} w={30} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        {isSelected ? (
          <EditableAccountItemContent
            account={props.account}
            userCurrency={props.userCurrency}
            toggle={toggle}
          />
        ) : (
          <AccountItemContent
            account={props.account}
            userCurrency={props.userCurrency}
            toggle={toggle}
          />
        )}
      </Group>
    </Card>
  );
};
export default AccountItem;
