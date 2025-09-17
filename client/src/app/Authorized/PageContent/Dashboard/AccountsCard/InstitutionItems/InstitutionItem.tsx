import { filterVisibleAccounts } from "~/helpers/accounts";
import { Card, Divider, Stack, Text } from "@mantine/core";
import { IAccountResponse } from "~/models/account";
import { IInstitution } from "~/models/institution";
import { DndContext } from "@dnd-kit/core";
import { SortableContext } from "@dnd-kit/sortable";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";

interface InstitutionItemProps {
  institution: IInstitution;
}

const InstitutionItem = (props: InstitutionItemProps): React.ReactNode => {
  const sortedFilteredAccounts = filterVisibleAccounts(
    props.institution.accounts
  ).sort((a, b) => a.index - b.index);

  return (
    <Card
      w="100%"
      bg="var(--mantine-color-card-alternate)"
      radius="lg"
      padding="sm"
      shadow="none"
    >
      <Stack gap={0.5}>
        <Text fw={600} size="md">
          {props.institution.name}
        </Text>
        <Divider mb="xs" size="sm" />
      </Stack>
      <Stack gap={1}>
        <DndContext>
          <SortableContext items={sortedFilteredAccounts}>
            {sortedFilteredAccounts.map((account: IAccountResponse) => (
              <AccountItem key={account.id} account={account} />
            ))}
          </SortableContext>
        </DndContext>
      </Stack>
    </Card>
  );
};

export default InstitutionItem;
