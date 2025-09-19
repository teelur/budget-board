import { Card, Group, Stack, Text } from "@mantine/core";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IInstitution } from "~/models/institution";
import AccountItem from "./AccountItem/AccountItem";

interface IInstitutionItemProps {
  institution: IInstitution;
  userCurrency: string;
}

const InstitutionItem = (props: IInstitutionItemProps) => {
  const totalBalance = props.institution.accounts
    .filter((a) => a.deleted === null)
    .reduce((acc, account) => acc + account.currentBalance, 0);
  return (
    <Card bg="var(--mantine-color-bg)" padding="0.5rem" radius="md" withBorder>
      <Stack gap="0.5rem">
        <Group justify="space-between" align="center">
          <Text fw={600} size="md">
            {props.institution.name}
          </Text>
          <Text fw={600} size="md" c={totalBalance < 0 ? "red" : "green"}>
            {convertNumberToCurrency(totalBalance, true, props.userCurrency)}
          </Text>
        </Group>
        <Stack gap="0.5rem">
          {props.institution.accounts
            .filter((a) => a.deleted === null)
            .map((account) => (
              <AccountItem
                key={account.id}
                account={account}
                userCurrency={props.userCurrency}
              />
            ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default InstitutionItem;
