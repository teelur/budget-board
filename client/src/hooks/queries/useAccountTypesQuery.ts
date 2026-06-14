import { notifications } from "@mantine/notifications";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { useTranslation } from "react-i18next";
import { accountTypesQueryKey } from "~/helpers/requests";
import { IAccountTypeResponse } from "~/models/accountType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

export const useAccountTypesQuery = () => {
  const { request } = useAuth();
  const { t } = useTranslation();

  return useQuery({
    queryKey: [accountTypesQueryKey],
    queryFn: async (): Promise<IAccountTypeResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/accountType",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccountTypeResponse[];
      }

      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("account_types_query_error"),
      });

      return [];
    },
  });
};
