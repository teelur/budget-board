import { LoadingOverlay, Stack } from "@mantine/core";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IInstitution, InstitutionIndexRequest } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import AccountDetails from "./AccountDetails/AccountDetails";
import { IAccountResponse } from "~/models/account";

interface AccountsContentProps {
  isSortable: boolean;
}

const AccountsContent = (props: AccountsContentProps) => {
  const [isDetailsOpen, { open: openDetails, close: closeDetails }] =
    useDisclosure(false);
  const [selectedAccount, setSelectedAccount] = React.useState<
    IAccountResponse | undefined
  >(undefined);

  const [sortedInstitutions, setSortedInstitutions] = React.useState<
    IInstitution[]
  >([]);

  const { request } = useAuth();
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
      // Some institutions might have conflicting indices, so we need to re-index them here
      // to ensure the drag-and-drop functionality works correctly
      setSortedInstitutions(
        institutionQuery.data
          .slice()
          .filter((inst) => inst.deleted === null)
          .sort((a, b) => a.index - b.index)
          .map((inst, index) => ({
            ...inst,
            index,
          }))
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
      <AccountDetails
        isOpen={isDetailsOpen}
        close={closeDetails}
        account={selectedAccount}
        currency={userSettingsQuery.data?.currency || "USD"}
      />
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
            openDetails={(account: IAccountResponse | undefined) => {
              setSelectedAccount(account);
              openDetails();
            }}
          />
        ))}
      </DragDropProvider>
    </Stack>
  );
};

export default AccountsContent;
