import { Accordion, Stack } from "@mantine/core";
import { IAccount } from "~/models/account";
import React from "react";
import DeletedAccountCard from "./DeletedAccountCard/DeletedAccountCard";

interface DeletedAccountsProps {
  deletedAccounts: IAccount[];
}

const DeletedAccounts = (props: DeletedAccountsProps): React.ReactNode => {
  const sortedDeletedAccounts = props.deletedAccounts.sort(
    (a, b) =>
      new Date(b.deleted ?? 0).getTime() - new Date(a.deleted ?? 0).getTime()
  );
  return (
    <Accordion variant="filled">
      <Accordion.Item
        value="deleted-account"
        bg="var(--mantine-color-accordion-alternate)"
      >
        <Accordion.Control>Deleted Accounts</Accordion.Control>
        <Accordion.Panel>
          <Stack>
            {sortedDeletedAccounts.map((deletedAccount) => (
              <DeletedAccountCard
                key={deletedAccount.id}
                deletedAccount={deletedAccount}
              />
            ))}
          </Stack>
        </Accordion.Panel>
      </Accordion.Item>
    </Accordion>
  );
};

export default DeletedAccounts;
