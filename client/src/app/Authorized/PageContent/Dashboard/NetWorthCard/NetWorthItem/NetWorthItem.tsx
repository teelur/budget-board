import { Group } from "@mantine/core";
import { convertNumberToCurrency } from "~/helpers/currency";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/Text/StatusText/StatusText";

interface NetWorthItemProps {
  title: string;
  totalBalance: number;
  userCurrency: string;
}

const NetWorthItem = (props: NetWorthItemProps): React.ReactNode => {
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  return (
    <Group p={0} justify="space-between" align="center" wrap="nowrap">
      <PrimaryText fw={600}>{props.title}</PrimaryText>
      {userSettingsQuery.isPending ? null : (
        <StatusText amount={props.totalBalance}>
          {convertNumberToCurrency(
            props.totalBalance,
            true,
            userSettingsQuery.data?.currency ?? "USD"
          )}
        </StatusText>
      )}
    </Group>
  );
};

export default NetWorthItem;
