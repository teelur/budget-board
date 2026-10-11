import { Group, Stack } from "@mantine/core";
import { BodyText, Card } from "@teelur/budget-board-ui";
import { ISimpleFinOrganizationResponse } from "~/models/simpleFinOrganization";
import SimpleFinAccountCard from "./SimpleFinAccountCard/SimpleFinAccountCard";

interface ISimpleFinOrganizationCardProps {
  simpleFinOrganization: ISimpleFinOrganizationResponse;
}

const SimpleFinOrganizationCard = (
  props: ISimpleFinOrganizationCardProps,
): React.ReactNode => {
  const sortedAccounts = props.simpleFinOrganization.accounts.sort((a, b) =>
    a.name.localeCompare(b.name),
  );

  return (
    <Card>
      <Stack p={0} gap="0.5rem">
        <Group gap="0.5rem">
          <BodyText tone="primary" size="md" fw={500}>
            {props.simpleFinOrganization.name ||
              props.simpleFinOrganization.domain}
          </BodyText>
          {props.simpleFinOrganization.simpleFinUrl && (
            <BodyText tone="secondary" size="sm">
              {props.simpleFinOrganization.simpleFinUrl}
            </BodyText>
          )}
        </Group>
        <Stack gap="0.5rem">
          {sortedAccounts.map((account) => (
            <SimpleFinAccountCard key={account.id} simpleFinAccount={account} />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default SimpleFinOrganizationCard;
