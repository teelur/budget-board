import { Accordion, Card, Drawer, Skeleton, Stack, Text } from "@mantine/core";
import { IGoalResponse } from "~/models/goal";
import React from "react";
import AccountItem from "~/components/AccountItem/AccountItem";
import { IAccount } from "~/models/account";

interface GoalDetailsProps {
  goal: IGoalResponse | null;
  isOpen: boolean;
  doClose: () => void;
}

const GoalDetails = (props: GoalDetailsProps): React.ReactNode => {
  return (
    <Drawer
      opened={props.isOpen}
      onClose={props.doClose}
      position="right"
      size="md"
      title={
        <Text size="lg" fw={600}>
          Goal Details
        </Text>
      }
    >
      {props.goal === null ? (
        <Skeleton height={425} radius="lg" />
      ) : (
        <Accordion variant="separated" defaultValue={["accounts"]} multiple>
          <Accordion.Item
            value="accounts"
            bg="var(--mantine-color-content-background)"
          >
            <Accordion.Control>
              <Text fw={600}>Accounts</Text>
            </Accordion.Control>
            <Accordion.Panel>
              <Stack gap="0.5rem">
                {props.goal.accounts.map((account: IAccount) => (
                  <Card key={account.id} radius="md">
                    <AccountItem account={account} />
                  </Card>
                ))}
              </Stack>
            </Accordion.Panel>
          </Accordion.Item>
        </Accordion>
      )}
    </Drawer>
  );
};

export default GoalDetails;
