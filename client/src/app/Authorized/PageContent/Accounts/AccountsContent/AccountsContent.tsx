import { LoadingOverlay, Stack } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { IInstitution, InstitutionIndexRequest } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { useDidUpdate } from "@mantine/hooks";

interface AccountsContentProps {
  isSortable: boolean;
}

const AccountsContent = (props: AccountsContentProps) => {
  const [sortedInstitutions, setSortedInstitutions] = React.useState<
    IInstitution[]
  >([]);

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
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data;
      }

      return undefined;
    },
  });

  React.useEffect(() => {
    if (institutionQuery.data) {
      setSortedInstitutions(
        institutionQuery.data.sort((a, b) => a.index - b.index)
      );
    }
  }, [institutionQuery.data]);

  const queryClient = useQueryClient();
  const doIndexInstitutions = useMutation({
    mutationFn: async (institutions: InstitutionIndexRequest[]) =>
      await request({
        url: "/api/institution/setindices",
        method: "PUT",
        data: institutions,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  useDidUpdate(() => {
    if (!props.isSortable) {
      const indexedInstitutions: InstitutionIndexRequest[] =
        sortedInstitutions.map((inst, index) => ({
          id: inst.id,
          index,
        }));
      doIndexInstitutions.mutate(indexedInstitutions);
    }
  }, [props.isSortable]);

  return (
    <Stack id="institutions-stack" gap="1rem">
      <LoadingOverlay visible={doIndexInstitutions.isPending} />
      <DragDropProvider
        onDragEnd={(event) => {
          const updatedList = move(sortedInstitutions, event).map(
            (inst, index) => ({
              ...inst,
              index,
            })
          );
          setSortedInstitutions(updatedList);
        }}
      >
        {sortedInstitutions.map((institution) => (
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
