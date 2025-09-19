import { Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IInstitution } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";
import { DragDropProvider } from "@dnd-kit/react";
import { arrayMove } from "@dnd-kit/helpers";

interface AccountsContentProps {
  sortedInstitutions: IInstitution[];
  setSortedInstitutions: React.Dispatch<React.SetStateAction<IInstitution[]>>;
  isSortable: boolean;
}

const AccountsContent = (props: AccountsContentProps) => {
  const { request } = React.useContext<any>(AuthContext);
  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async () => {
      const res = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data;
      }

      return undefined;
    },
  });

  return (
    <Stack id="institutions-stack" gap="1rem">
      <DragDropProvider
        onDragEnd={(event) => {
          const fromIndex = event.operation.source?.data.index;
          const toIndex = event.operation.target?.data.index;

          if (fromIndex === undefined || toIndex === undefined) {
            return;
          }

          props.setSortedInstitutions((items) =>
            arrayMove(items, fromIndex, toIndex)
          );
        }}
      >
        {props.sortedInstitutions.map((institution) => (
          <InstitutionItem
            key={institution.id}
            institution={institution}
            userCurrency={userSettingsQuery.data?.currency || "USD"}
            isSortable={props.isSortable}
            container={document.getElementById("institutions-stack") as Element}
          />
        ))}
      </DragDropProvider>
    </Stack>
  );
};

export default AccountsContent;
