import { Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItems/InstitutionItem";
import { IAccountResponse } from "~/models/account";
import Card from "~/components/Card/Card";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/Text/DimmedText/DimmedText";

const AccountsCard = (): React.ReactNode => {
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
    [institutionQuery.data]
  );

  const sortedFilteredInstitutionsForDisplay = React.useMemo(
    () =>
      sortedFilteredInstitutions.filter(
        (i) =>
          i.accounts.filter(
            (a) => a.deleted === null && a.hideAccount === false
          ).length > 0
      ),
    [sortedFilteredInstitutions]
  );

  return (
    <Card w="100%" elevation={1}>
      <Stack gap="0.5rem">
        <PrimaryText size="xl">Accounts</PrimaryText>
        <Stack align="center" gap="0.5rem">
          {institutionQuery.isPending || accountsQuery.isPending ? (
            <Skeleton height={600} radius="lg" />
          ) : (sortedFilteredInstitutionsForDisplay ?? []).length > 0 ? (
            (sortedFilteredInstitutionsForDisplay ?? []).map(
              (institution: IInstitution) => (
                <InstitutionItem
                  key={institution.id}
                  institution={institution}
                />
              )
            )
          ) : (
            <DimmedText size="md">No accounts found</DimmedText>
          )}
        </Stack>
      </Stack>
    </Card>
  );
};

export default AccountsCard;
