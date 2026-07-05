import { ActionIcon, Button, Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { PlusIcon } from "lucide-react";
import { areStringsEqual } from "~/helpers/utils";
import { AccountSource, IAccountCreateRequest } from "~/models/account";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import Autocomplete from "~/components/core/Autocomplete/Autocomplete";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useCreateAccountMutation } from "~/hooks/mutations/accounts/useCreateAccountMutation";
import { useInstitutionsQuery } from "~/hooks/queries/useInstitutionsQuery";
import { useCreateInstitutionMutation } from "~/hooks/mutations/institutions/useCreateInstitutionMutation";

const CreateAccount = () => {
  const [opened, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();
  const institutionQuery = useInstitutionsQuery();
  const createAccountMutation = useCreateAccountMutation();
  const createInstitutionMutation = useCreateInstitutionMutation();

  const accountNameField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("name_is_required")),
  });

  const institutionField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("institution_is_required")),
  });

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
      await createInstitutionMutation.mutateAsync({
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

    createAccountMutation.mutate(
      {
        name: accountNameField.getValue(),
        institutionID: institutionForAccount.id,
        source: AccountSource.Manual,
      } as IAccountCreateRequest,
      {
        onSuccess: () => {
          accountNameField.reset();
          institutionField.reset();
        },
      },
    );
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
              createAccountMutation.isPending ||
              createInstitutionMutation.isPending
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
