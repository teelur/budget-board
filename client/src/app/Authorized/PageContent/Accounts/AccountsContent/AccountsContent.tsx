import { Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";

const AccountsContent = () => {
  const { request } = React.useContext<any>(AuthContext);
  const institutionQuery = useQuery({
    queryKey: ["institutions"],
    queryFn: async () => {
      const res = await request({
        url: "/api/institution",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IInstitution[];
      }

      return undefined;
    },
  });

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async () => {
      const res = await request({
        url: "/api/user/settings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data;
      }

      return undefined;
    },
  });
  return (
    <Stack>
      {institutionQuery.data?.map((institution) => (
        <InstitutionItem
          key={institution.id}
          institution={institution}
          userCurrency={userSettingsQuery.data?.currency || "USD"}
        />
      ))}
    </Stack>
  );
};

export default AccountsContent;
