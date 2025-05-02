import { Button, Modal, Stack, TextInput } from "@mantine/core";
import { isNotEmpty, useForm } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { areStringsEqual } from "~/helpers/utils";
import { AccountSource, IAccountCreateRequest } from "~/models/account";
import { IInstitution, IInstitutionCreateRequest } from "~/models/institution";

interface CreateAccountModalProps {
  opened: boolean;
  onClose: () => void;
  stackId: string;
}

interface formValues {
  name: string;
  institution: string;
}

const CreateAccountModal = (props: CreateAccountModalProps) => {
  const form = useForm<formValues>({
    mode: "uncontrolled",
    initialValues: {
      name: "",
      institution: "",
    },

    validate: {
      name: isNotEmpty("name is required"),
      institution: isNotEmpty("institution is required"),
    },
  });

  const { request } = React.useContext<any>(AuthContext);
  const institutionQuery = useQuery({
    queryKey: ["institutions"],
    queryFn: async (): Promise<IInstitution[]> => {
      const res: AxiosResponse = await request({
        url: "/api/institution",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IInstitution[];
      }

      return [];
    },
  });

  const queryClient = useQueryClient();
  const doCreateInstitution = useMutation({
    mutationFn: async (newInstitution: IInstitutionCreateRequest) =>
      await request({
        url: "/api/institution",
        method: "POST",
        data: newInstitution,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  const doCreateAccount = useMutation({
    mutationFn: async (newAccount: IAccountCreateRequest) =>
      await request({
        url: "/api/account",
        method: "POST",
        data: newAccount,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
    },
  });

  const onCreateAccount = async (values: formValues) => {
    if (
      institutionQuery.data?.filter((i) =>
        areStringsEqual(i.name, values.institution)
      ).length === 0
    ) {
      await doCreateInstitution.mutateAsync({
        name: values.institution,
      });
    }

    const institutionsResponse: AxiosResponse = await request({
      url: "/api/institution",
      method: "GET",
    });

    if (institutionsResponse.status !== 200 || !institutionsResponse.data) {
      notifications.show({
        message: "There was an error creating the institution",
        color: "red",
      });
      return;
    }

    const institution = (institutionsResponse.data as IInstitution[]).filter(
      (i) => areStringsEqual(i.name, values.institution)
    );
    if (institution.length !== 1) {
      notifications.show({
        message: "There was an error getting the created institution",
        color: "red",
      });
      return;
    }

    await doCreateAccount.mutateAsync({
      name: values.name,
      institutionID: institution[0]!.id,
      source: AccountSource.Manual,
    } as IAccountCreateRequest);
    form.reset();
    props.onClose();
  };

  if (institutionQuery.isPending) {
    return null;
  }

  return (
    <Modal
      opened={props.opened}
      onClose={props.onClose}
      stackId={props.stackId}
      title="Create Account"
      styles={{
        inner: {
          left: "0",
          right: "0",
          padding: "0 !important",
        },
      }}
    >
      <Stack>
        <form
          style={{ width: "100%" }}
          onSubmit={form.onSubmit((values) => onCreateAccount(values))}
        >
          <Stack gap={10}>
            <TextInput {...form.getInputProps("name")} label="Account Name" />
            <TextInput
              {...form.getInputProps("institution")}
              label="Institution"
            />
            <Button
              type="submit"
              loading={
                doCreateAccount.isPending || doCreateInstitution.isPending
              }
            >
              Submit
            </Button>
          </Stack>
        </form>
      </Stack>
    </Modal>
  );
};

export default CreateAccountModal;
