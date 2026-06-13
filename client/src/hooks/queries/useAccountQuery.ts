import { notifications } from "@mantine/notifications";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { useTranslation } from "react-i18next";
import { accountsQueryKey } from "~/helpers/requests";
import { IAccountResponse } from "~/models/account";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useAccountsQuery = ({
  enabled = true,
}: { enabled?: boolean } = {}) => {
  const { request } = useAuth();
  const { t } = useTranslation();

  return useQuery({
    queryKey: [accountsQueryKey],
    queryFn: async (): Promise<IAccountResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountResponse[];
      }

      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("accounts_query_error"),
      });

      return [];
    },
    enabled,
  });
};
