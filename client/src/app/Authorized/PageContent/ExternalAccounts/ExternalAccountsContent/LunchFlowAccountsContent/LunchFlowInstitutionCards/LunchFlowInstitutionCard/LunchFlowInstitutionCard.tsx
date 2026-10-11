import { Group, Stack } from "@mantine/core";
import { Card, HeadingText } from "@teelur/budget-board-ui";
import { IGroupedLunchFlowAccounts } from "../LunchFlowInstitutionCards";
import LunchFlowAccountCard from "./LunchFlowAccountCard/LunchFlowAccountCard";

interface ILunchFlowInstitutionCardProps {
  lunchFlowInstitution: IGroupedLunchFlowAccounts;
}

const LunchFlowInstitutionCard = (
  props: ILunchFlowInstitutionCardProps,
): React.ReactNode => {
  const sortedAccounts = props.lunchFlowInstitution.accounts.sort((a, b) =>
    a.name.localeCompare(b.name),
  );

  return (
    <Card>
      <Stack p={0} gap="0.5rem">
        <Group gap="0.5rem">
          <HeadingText tone="primary" level={5}>
            {props.lunchFlowInstitution.institutionName}
          </HeadingText>
        </Group>
        <Stack gap="0.5rem">
          {sortedAccounts.map((account) => (
            <LunchFlowAccountCard key={account.id} lunchFlowAccount={account} />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default LunchFlowInstitutionCard;
