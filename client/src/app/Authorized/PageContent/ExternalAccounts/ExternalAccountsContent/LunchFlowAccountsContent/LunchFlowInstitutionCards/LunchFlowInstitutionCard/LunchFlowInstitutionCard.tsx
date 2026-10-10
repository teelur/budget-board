import { Group, Stack } from "@mantine/core";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { IGroupedLunchFlowAccounts } from "../LunchFlowInstitutionCards";
import LunchFlowAccountCard from "./LunchFlowAccountCard/LunchFlowAccountCard";
import { Card } from "@teelur/budget-board-ui";

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
          <PrimaryText size="md">
            {props.lunchFlowInstitution.institutionName}
          </PrimaryText>
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
