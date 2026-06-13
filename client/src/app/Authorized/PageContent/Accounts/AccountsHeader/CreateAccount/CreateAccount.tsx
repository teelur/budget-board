import { ActionIcon, Button, Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { PlusIcon } from "lucide-react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError, institutionsQueryKey } from "~/helpers/requests";
import { areStringsEqual } from "~/helpers/utils";
import { AccountSource, IAccountCreateRequest } from "~/models/account";
import { IInstitution, IInstitutionCreateRequest } from "~/models/institution";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import Autocomplete from "~/components/core/Autocomplete/Autocomplete";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useCreateAccountMutation } from "~/hooks/mutations/accounts/useCreateAccountMutation";

const CreateAccount = () => {
  const [opened, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();

  const accountNameField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("name_is_required")),
  });

  const institutionField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("institution_is_required")),
  });

  const { request } = useAuth();

  const institutionQuery = useQuery({
    queryKey: [institutionsQueryKey],
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

  const doCreateInstitution = useMutation({
    mutationFn: async (newInstitution: IInstitutionCreateRequest) =>
      await request({
        url: "/api/institution",
        method: "POST",
        data: newInstitution,
      }),
    // Purposely not refeching here since we will refetch in the account creation flow
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  const createAccountMutation = useCreateAccountMutation();

  const onCreateAccount = async () => {
    await accountNameField.validate();
    await institutionField.validate();

    if (
      accountNameField.getValue().length === 0 ||
      institutionField.getValue().length === 0
    ) {
      return;
    }

    let institutionForAccount = institutionQuery.data?.find((i) =>
      areStringsEqual(i.name, institutionField.getValue()),
    );

    if (institutionForAccount === undefined) {
      await doCreateInstitution.mutateAsync({
        name: institutionField.getValue(),
      });

      const institutionQueryResult = await institutionQuery.refetch();

      institutionForAccount = institutionQueryResult.data?.find((i) =>
        areStringsEqual(i.name, institutionField.getValue()),
      );

      if (institutionForAccount === undefined) {
        notifications.show({
          message: t("institution_creation_failed_message"),
          color: "var(--button-color-destructive)",
        });
        return;
      }
    }

    createAccountMutation.mutate({
      name: accountNameField.getValue(),
      institutionID: institutionForAccount.id,
      source: AccountSource.Manual,
    } as IAccountCreateRequest);
  };
  if (createAccountMutation.isSuccess) {
    accountNameField.reset();
    institutionField.reset();
  }

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
        title={
          <PrimaryHeading component="span" order={4}>
            {t("create_account")}
          </PrimaryHeading>
        }
      >
        <Stack gap="0.5rem">
          <TextInput
            {...accountNameField.getInputProps()}
            label={<PrimaryText size="sm">{t("account_name")}</PrimaryText>}
            elevation={0}
          />
          <Autocomplete
            {...institutionField.getInputProps()}
            label={<PrimaryText size="sm">{t("institution")}</PrimaryText>}
            data={
              institutionQuery.data
                ? institutionQuery.data.map((i) => i.name)
                : []
            }
            clearable
            elevation={0}
          />
          <Button
            loading={
              createAccountMutation.isPending || doCreateInstitution.isPending
            }
            onClick={onCreateAccount}
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default CreateAccount;
