import { ActionIcon, Button, Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { PlusIcon } from "lucide-react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { areStringsEqual } from "~/helpers/utils";
import { AccountSource, IAccountCreateRequest } from "~/models/account";
import { IInstitution, IInstitutionCreateRequest } from "~/models/institution";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import BaseTextInput from "~/components/core/Input/Base/BaseTextInput/BaseTextInput";

const CreateAccount = () => {
  const [opened, { open, close }] = useDisclosure(false);

  const accountNameField = useField<string>({
    initialValue: "",
    validate: isNotEmpty("Name is required"),
  });

  const institutionField = useField<string>({
    initialValue: "",
    validate: isNotEmpty("Institution is required"),
  });

  const { request } = useAuth();
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
    // Purposely not refeching here since we will refetch in the account creation flow
    onError: (error: AxiosError) => {
      if (
        error.message.includes("An institution with this name already exists.")
      ) {
        return;
      }

      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
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

      notifications.show({
        message: "Account created",
        color: "var(--button-color-confirm)",
      });

      accountNameField.reset();
      institutionField.reset();
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  const onCreateAccount = async () => {
    let institutionForAccount = institutionQuery.data?.find((i) =>
      areStringsEqual(i.name, institutionField.getValue())
    );

    if (institutionForAccount === undefined) {
      await doCreateInstitution.mutateAsync({
        name: institutionField.getValue(),
      });

      const institutionQueryResult = await institutionQuery.refetch();

      institutionForAccount = institutionQueryResult.data?.find((i) =>
        areStringsEqual(i.name, institutionField.getValue())
      );

      if (institutionForAccount === undefined) {
        notifications.show({
          message: "Failed to create institution for account",
          color: "var(--button-color-destructive)",
        });
        return;
      }
    }

    doCreateAccount.mutate({
      name: accountNameField.getValue(),
      institutionID: institutionForAccount.id,
      source: AccountSource.Manual,
    } as IAccountCreateRequest);
  };

  if (institutionQuery.isPending) {
    return null;
  }

  return (
    <>
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={opened}
        onClose={close}
        title={<PrimaryText size="md">Create Account</PrimaryText>}
      >
        <Stack gap="0.5rem">
          <BaseTextInput
            {...accountNameField.getInputProps()}
            label={<PrimaryText size="sm">Account Name</PrimaryText>}
          />
          <BaseTextInput
            {...institutionField.getInputProps()}
            label={<PrimaryText size="sm">Institution</PrimaryText>}
          />
          <Button
            type="submit"
            loading={doCreateAccount.isPending || doCreateInstitution.isPending}
            onClick={onCreateAccount}
          >
            Submit
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default CreateAccount;
