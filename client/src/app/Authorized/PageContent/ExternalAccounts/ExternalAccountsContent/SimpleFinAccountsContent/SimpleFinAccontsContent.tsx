import { Group, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import React from "react";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { ISimpleFinOrganizationResponse } from "~/models/simpleFinOrganization";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import SimpleFinOrganizationCard from "./SimpleFinOrganizationCard/SimpleFinOrganizationCard";

const SimpleFinAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const simpleFinOrganizationsQuery = useQuery({
    queryKey: ["simpleFinOrganizations"],
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

  const organizations = simpleFinOrganizationsQuery.data ?? [];

  return (
    <Stack p={0} gap="0.5rem">
      <PrimaryText size="lg">{t("simplefin")}</PrimaryText>
      <Stack gap="0.5rem">
        {organizations.length > 0 ? (
          organizations.map((organization) => (
            <SimpleFinOrganizationCard
              key={organization.id}
              simpleFinOrganization={organization}
            />
          ))
        ) : (
          <Group justify="center">
            <DimmedText size="sm">
              {t("no_simplefin_accounts_found")}
            </DimmedText>
          </Group>
        )}
      </Stack>
    </Stack>
  );
};

export default SimpleFinAccountsContent;
