import { Group, Stack } from "@mantine/core";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { ISimpleFinOrganizationResponse } from "~/models/simpleFinOrganization";
import SimpleFinAccountCard from "./SimpleFinAccountCard/SimpleFinAccountCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

interface ISimpleFinOrganizationCardProps {
  simpleFinOrganization: ISimpleFinOrganizationResponse;
}

const SimpleFinOrganizationCard = (
  props: ISimpleFinOrganizationCardProps
): React.ReactNode => {
  return (
    <Card elevation={1}>
      <Stack p={0} gap="0.5rem">
        <Group gap="0.5rem">
          {props.simpleFinOrganization.name ? (
            <PrimaryText size="md">
              {props.simpleFinOrganization.name}
            </PrimaryText>
          ) : (
            <PrimaryText size="md">
              {props.simpleFinOrganization.domain}
            </PrimaryText>
          )}
          {props.simpleFinOrganization.domain && (
            <DimmedText size="sm">
              {props.simpleFinOrganization.simpleFinUrl}
            </DimmedText>
          )}
        </Group>
        <Stack gap="0.5rem">
          {props.simpleFinOrganization.accounts.map((account) => (
            <SimpleFinAccountCard key={account.id} simpleFinAccount={account} />
          ))}
        </Stack>
      </Stack>
    </Card>
  );
};

export default SimpleFinOrganizationCard;
