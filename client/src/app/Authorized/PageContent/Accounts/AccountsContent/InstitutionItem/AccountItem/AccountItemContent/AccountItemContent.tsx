import { ActionIcon, Badge, Group, Stack, Text } from "@mantine/core";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAccountResponse } from "~/models/account";

interface IAccountItemContentProps {
  account: IAccountResponse;
  userCurrency: string;
  toggle: () => void;
}

const AccountItemContent = (props: IAccountItemContentProps) => {
  return (
    <Stack gap={0} flex="1 1 auto">
      <Group justify="space-between" align="center">
        <Group gap="0.5rem" align="center">
          <Text fw={600} size="md">
            {props.account.name}
          </Text>
          <ActionIcon
            variant="transparent"
            size="md"
            onClick={(e) => {
              e.stopPropagation();
              props.toggle();
            }}
          >
            <PencilIcon size={16} />
          </ActionIcon>
          <Badge>
            Interest Rate:{" "}
            {((props.account.interestRate ?? 0) * 100).toFixed(2)}%
          </Badge>
          {props.account.hideAccount && <Badge bg="yellow">Hidden</Badge>}
          {props.account.hideTransactions && (
            <Badge bg="purple">Hidden Transactions</Badge>
          )}
          {props.account.syncID !== null && <Badge bg="blue">SimpleFIN</Badge>}
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
          {props.account.subtype ? props.account.subtype : props.account.type}
        </Text>
        <Text fw={600} size="sm" c="dimmed">
          Last Updated:{" "}
          {(() => {
            const parsedDate = dayjs(props.account.balanceDate);
            return parsedDate.isValid() ? parsedDate.format("L LT") : "Never!";
          })()}
        </Text>
      </Group>
    </Stack>
  );
};

export default AccountItemContent;
