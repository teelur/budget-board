import classes from "./AccountsCard.module.css";

import { Card, Group, Skeleton, Stack, Title, Text } from "@mantine/core";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItems/InstitutionItem";
import { IAccountResponse } from "~/models/account";
import AccountsSettings from "./AccountsSettings/AccountsSettings";

const AccountsCard = (): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);
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
    <Card
      className={classes.card}
      padding="xs"
      radius="md"
      shadow="sm"
      withBorder
    >
      <Group className={classes.headerContainer}>
        <Title order={2}>Accounts</Title>
        <AccountsSettings
          sortedFilteredInstitutions={sortedFilteredInstitutions}
          accounts={accountsQuery.data ?? []}
        />
      </Group>
      <Stack className={classes.accountsContainer}>
        {institutionQuery.isPending || accountsQuery.isPending ? (
          <Skeleton height={600} radius="lg" />
        ) : (sortedFilteredInstitutionsForDisplay ?? []).length > 0 ? (
          (sortedFilteredInstitutionsForDisplay ?? []).map(
            (institution: IInstitution) => (
              <InstitutionItem key={institution.id} institution={institution} />
            )
          )
        ) : (
          <Text>No accounts found</Text>
        )}
      </Stack>
    </Card>
  );
};

export default AccountsCard;
