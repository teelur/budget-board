import { Group, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { lunchFlowAccountQueryKey } from "~/helpers/requests";
import { ILunchFlowAccountResponse } from "~/models/lunchFlowAccount";
import LunchFlowInstitutionCard from "./LunchFlowInstitutionCard/LunchFlowInstitutionCard";

export interface IGroupedLunchFlowAccounts {
  institutionName: string;
  accounts: ILunchFlowAccountResponse[];
}

const LunchFlowInstitutionCards = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();

  const lunchFlowAccountQuery = useQuery({
    queryKey: [lunchFlowAccountQueryKey],
    queryFn: async () => {
      const res = await request({
        url: "/api/lunchFlowAccount",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ILunchFlowAccountResponse[];
      }

      return undefined;
    },
  });

  const lunchFlowAccountsGroupedByInstitution = () => {
    const groupedLunchFlowAccounts: IGroupedLunchFlowAccounts[] = [];

    if (lunchFlowAccountQuery.data) {
      const institutionNames = Array.from(
        new Set(
          lunchFlowAccountQuery.data.map((account) => account.institutionName),
        ),
      );

      institutionNames.forEach((institutionName) => {
        const instAccounts = lunchFlowAccountQuery.data!.filter(
          (account) => account.institutionName === institutionName,
        );

        if (instAccounts.length > 0) {
          groupedLunchFlowAccounts.push({
            institutionName,
            accounts: instAccounts,
          });
        }
      });
    }

    return groupedLunchFlowAccounts;
  };

  return (
    <Stack gap="0.5rem">
      {lunchFlowAccountQuery.data && lunchFlowAccountQuery.data.length > 0 ? (
        lunchFlowAccountsGroupedByInstitution().map((institution) => (
          <LunchFlowInstitutionCard
            key={institution.institutionName}
            lunchFlowInstitution={institution}
          />
        ))
      ) : (
        <Group justify="center">
          <DimmedText size="sm">{t("no_lunchflow_accounts_found")}</DimmedText>
        </Group>
      )}
    </Stack>
  );
};

export default LunchFlowInstitutionCards;
