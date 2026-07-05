import { Group, LoadingOverlay, Skeleton, Stack } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IInstitution, InstitutionIndexRequest } from "~/models/institution";
import InstitutionItem from "./InstitutionItem/InstitutionItem";
import { DragDropProvider } from "@dnd-kit/react";
import { move } from "@dnd-kit/helpers";
import { userSettingsQueryKey } from "~/helpers/requests";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import AccountDetails from "./AccountDetails/AccountDetails";
import { IAccountResponse } from "~/models/account";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { InfoIcon } from "lucide-react";
import { useInstitutionsQuery } from "~/hooks/queries/useInstitutionsQuery";
import { useOrderInstitutionsMutation } from "~/hooks/mutations/institutions/useOrderInstitutionMutation";

interface AccountsContentProps {
  isSortable: boolean;
}

const AccountsContent = (props: AccountsContentProps) => {
  const [isDetailsOpen, { open: openDetails, close: closeDetails }] =
    useDisclosure(false);
  const [selectedAccount, setSelectedAccount] = React.useState<
    IAccountResponse | undefined
  >(undefined);

  const { t } = useTranslation();
  const institutionQuery = useInstitutionsQuery();
  const orderInstitutionsMutation = useOrderInstitutionsMutation();

  const [sortedInstitutions, setSortedInstitutions] = React.useState<
    IInstitution[]
  >([]);

  const { request } = useAuth();
  const userSettingsQuery = useQuery({
    queryKey: [userSettingsQueryKey],
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
          })),
      );
    }
  }, [institutionQuery.data]);

  useDidUpdate(() => {
    if (!props.isSortable) {
      const orderedInstitutions: InstitutionIndexRequest[] =
        sortedInstitutions.map((inst, index) => ({
          id: inst.id,
          index,
        }));
      orderInstitutionsMutation.mutate(orderedInstitutions);
    }
  }, [props.isSortable]);

  return (
    <Stack id="institutions-stack" gap="1rem">
      <LoadingOverlay visible={orderInstitutionsMutation.isPending} />
      {selectedAccount && (
        <AccountDetails
          isOpen={isDetailsOpen}
          close={closeDetails}
          account={selectedAccount}
          currency={userSettingsQuery.data?.currency || "USD"}
        />
      )}
      {institutionQuery.isPending ? (
        <>
          <Skeleton height={60} radius="md" />
          <Skeleton height={60} radius="md" />
          <Skeleton height={60} radius="md" />
        </>
      ) : sortedInstitutions.length === 0 ? (
        <Group justify="center" align="center" gap="0.5rem">
          <InfoIcon size={20} color="var(--base-color-text-dimmed)" />
          <DimmedText size="sm">{t("no_institutions")}</DimmedText>
        </Group>
      ) : (
        <DragDropProvider
          onDragEnd={(event) => {
            const updatedList = move(sortedInstitutions, event).map(
              (inst, index) => ({
                ...inst,
                index,
              }),
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
              container={
                document.getElementById("institutions-stack") as Element
              }
              openDetails={(account: IAccountResponse | undefined) => {
                setSelectedAccount(account);
                openDetails();
              }}
            />
          ))}
        </DragDropProvider>
      )}
    </Stack>
  );
};

export default AccountsContent;
