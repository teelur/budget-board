import { Divider, Group } from "@mantine/core";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface NetWorthItemProps {
  title: string;
  totalBalance: number;
  userCurrency: string;
}

const NetWorthItem = (props: NetWorthItemProps): React.ReactNode => {
  const { intlLocale } = useLocale();
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
    <Group p={0} align="center" wrap="nowrap" gap="0.25rem">
      <PrimaryText fw={600}>{props.title}</PrimaryText>
      <Divider
        color="var(--elevated-color-border)"
        my="sm"
        size="xs"
        variant="dashed"
        flex="1 0 auto"
      />
      {userSettingsQuery.isPending ? null : (
        <StatusText amount={props.totalBalance}>
          {convertNumberToCurrency(
            props.totalBalance,
            true,
            userSettingsQuery.data?.currency ?? "USD",
            SignDisplay.Auto,
            intlLocale,
          )}
        </StatusText>
      )}
    </Group>
  );
};

export default NetWorthItem;
