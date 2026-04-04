import { Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItems/InstitutionItem";
import { IAccountResponse } from "~/models/account";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import SplitCard, {
  BorderThickness,
} from "~/components/ui/SplitCard/SplitCard";
import { LandmarkIcon } from "lucide-react";

const AccountsCard = (): React.ReactNode => {
  const { t } = useTranslation();

  const { request } = useAuth();
  const institutionQuery = useQuery({
    queryKey: ["institutions"],
    queryFn: async (): Promise<IInstitution[]> => {
      const res: AxiosResponse = await request({
        url: "/api/institution",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IInstitution[];
      }

      return [];
    },
  });

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      return [];
    },
  });

  const sortedFilteredInstitutions = React.useMemo(
    () =>
      (institutionQuery.data ?? [])
        .filter((i) => i.deleted === null)
        .sort((a, b) => a.index - b.index),
    [institutionQuery.data],
  );

  const sortedFilteredInstitutionsForDisplay = React.useMemo(
    () =>
      sortedFilteredInstitutions.filter(
        (i) =>
          i.accounts.filter(
            (a) => a.deleted === null && a.hideAccount === false,
          ).length > 0,
      ),
    [sortedFilteredInstitutions],
  );

  return (
    <SplitCard
      w="100%"
      border={BorderThickness.Thick}
      header={
        <Group gap={"0.25rem"}>
          <LandmarkIcon color="var(--base-color-text-dimmed)" />
          <PrimaryText size="xl" lh={1}>
            {t("accounts")}
          </PrimaryText>
        </Group>
      }
      elevation={1}
    >
      <Stack align="center" gap="0.5rem">
        {institutionQuery.isPending || accountsQuery.isPending ? (
          <Skeleton height={600} radius="lg" />
        ) : (sortedFilteredInstitutionsForDisplay ?? []).length > 0 ? (
          (sortedFilteredInstitutionsForDisplay ?? []).map(
            (institution: IInstitution) => (
              <InstitutionItem key={institution.id} institution={institution} />
            ),
          )
        ) : (
          <DimmedText size="sm">{t("no_accounts_found")}</DimmedText>
        )}
      </Stack>
    </SplitCard>
  );
};

export default AccountsCard;
