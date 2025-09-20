import { useSortable } from "@dnd-kit/react/sortable";
import { Badge, Button, Card, Flex, Group, Stack, Text } from "@mantine/core";
import dayjs from "dayjs";
import { GripVertical } from "lucide-react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAccountResponse } from "~/models/account";
import { RestrictToVerticalAxis } from "@dnd-kit/abstract/modifiers";
import { RestrictToElement } from "@dnd-kit/dom/modifiers";
import { closestCenter } from "@dnd-kit/collision";

interface IAccountItemProps {
  account: IAccountResponse;
  userCurrency: string;
  isSortable: boolean;
  container: Element;
}

const AccountItem = (props: IAccountItemProps) => {
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
    <Card ref={ref} shadow="sm" padding="0.5rem" radius="md" withBorder>
      <Group>
        {props.isSortable && (
          <Flex ref={handleRef} style={{ alignSelf: "stretch" }}>
            <Button h="100%" px={0} w={30} radius="lg">
              <GripVertical size={25} />
            </Button>
          </Flex>
        )}
        <Stack gap={0} flex="1 1 auto">
          <Group justify="space-between" align="center">
            <Group>
              <Text fw={600} size="md">
                {props.account.name}
              </Text>
              <Badge>
                Interest Rate:{" "}
                {((props.account.interestRate ?? 0) * 100).toFixed(2)}%
              </Badge>
              {props.account.hideAccount && <Badge bg="yellow">Hidden</Badge>}
              {props.account.deleted && <Badge bg="red">Deleted</Badge>}
            </Group>
            <Text
              fw={600}
              size="md"
              c={props.account.currentBalance < 0 ? "red" : "green"}
            >
              {convertNumberToCurrency(
                props.account.currentBalance,
                true,
                props.userCurrency
              )}
            </Text>
          </Group>
          <Group justify="space-between" align="center">
            <Text fw={600} size="sm" c="dimmed">
              {props.account.subtype
                ? props.account.subtype
                : props.account.type}
            </Text>
            <Text fw={600} size="sm" c="dimmed">
              Last Updated: {dayjs(props.account.balanceDate).format("L LT")}
            </Text>
          </Group>
        </Stack>
      </Group>
    </Card>
  );
};
export default AccountItem;
