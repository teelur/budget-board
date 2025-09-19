import { LoadingOverlay, Stack } from "@mantine/core";
import React from "react";
import AccountsContent from "./AccountsContent/AccountsContent";
import AccountsHeader from "./AccountsHeader/AccountsHeader";
import { useDisclosure } from "@mantine/hooks";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { IInstitution, InstitutionIndexRequest } from "~/models/institution";
import { AxiosError } from "axios";
import { translateAxiosError } from "~/helpers/requests";
import { notifications } from "@mantine/notifications";

const Accounts = (): React.ReactNode => {
  const [isSortable, { toggle }] = useDisclosure(false);

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
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      toggle();
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  const onReorderClick = () => {
    if (isSortable) {
      const indexedInstitutions: InstitutionIndexRequest[] =
        sortedInstitutions.map((inst, index) => ({
          id: inst.id,
          index,
        }));
      doIndexInstitutions.mutate(indexedInstitutions);
    } else {
      toggle();
    }
  };

  return (
    <Stack w="100%" maw={1400}>
      <LoadingOverlay visible={doIndexInstitutions.isPending} />
      <AccountsHeader isSortable={isSortable} toggleSort={onReorderClick} />
      <AccountsContent
        sortedInstitutions={sortedInstitutions}
        setSortedInstitutions={setSortedInstitutions}
        isSortable={isSortable}
      />
    </Stack>
  );
};

export default Accounts;
