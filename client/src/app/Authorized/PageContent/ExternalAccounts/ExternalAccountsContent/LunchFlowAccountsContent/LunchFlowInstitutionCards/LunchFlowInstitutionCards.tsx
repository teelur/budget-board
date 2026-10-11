import { Group, Stack } from "@mantine/core";
import { useTranslation } from "react-i18next";
import { ILunchFlowAccountResponse } from "~/models/lunchFlowAccount";
import LunchFlowInstitutionCard from "./LunchFlowInstitutionCard/LunchFlowInstitutionCard";
import { useLunchFlowAccountsQuery } from "~/hooks/queries/useLunchFlowAccountsQuery";
import { BodyText } from "@teelur/budget-board-ui";

export interface IGroupedLunchFlowAccounts {
  institutionName: string;
  accounts: ILunchFlowAccountResponse[];
}

const LunchFlowInstitutionCards = (): React.ReactNode => {
  const { t } = useTranslation();
  const lunchFlowAccountQuery = useLunchFlowAccountsQuery();

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
          <BodyText tone="muted" size="sm">
            {t("no_lunchflow_accounts_found")}
          </BodyText>
        </Group>
      )}
    </Stack>
  );
};

export default LunchFlowInstitutionCards;
