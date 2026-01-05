import { Group, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { ISimpleFinOrganizationResponse } from "~/models/simpleFinOrganization";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import SimpleFinOrganizationCard from "./SimpleFinOrganizationCard/SimpleFinOrganizationCard";
import { simpleFinOrganizationQueryKey } from "~/helpers/requests";

const SimpleFinOrganizationCards = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const simpleFinOrganizationsQuery = useQuery({
    queryKey: [simpleFinOrganizationQueryKey],
    queryFn: async () => {
      const res = await request({
        url: "/api/simpleFinOrganization",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ISimpleFinOrganizationResponse[];
      }

      return undefined;
    },
  });

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
