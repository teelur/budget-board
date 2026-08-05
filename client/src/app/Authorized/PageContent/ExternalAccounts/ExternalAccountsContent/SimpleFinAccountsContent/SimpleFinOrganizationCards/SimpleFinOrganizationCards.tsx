import { Group, Stack } from "@mantine/core";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import SimpleFinOrganizationCard from "./SimpleFinOrganizationCard/SimpleFinOrganizationCard";
import { useSimpleFinOrganizationsQuery } from "~/hooks/queries/useSimpleFinOrganizationsQuery";

const SimpleFinOrganizationCards = (): React.ReactNode => {
  const { t } = useTranslation();
  const simpleFinOrganizationsQuery = useSimpleFinOrganizationsQuery();

  return (
    <Stack gap="0.5rem">
      {simpleFinOrganizationsQuery.data &&
      simpleFinOrganizationsQuery.data.length > 0 ? (
        simpleFinOrganizationsQuery.data.map((organization) => (
          <SimpleFinOrganizationCard
            key={organization.id}
            simpleFinOrganization={organization}
          />
        ))
      ) : (
        <Group justify="center">
          <DimmedText size="sm">{t("no_simplefin_accounts_found")}</DimmedText>
        </Group>
      )}
    </Stack>
  );
};

export default SimpleFinOrganizationCards;
