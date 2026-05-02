import { Button, LoadingOverlay, SegmentedControl, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { accountTypesQueryKey, translateAxiosError } from "~/helpers/requests";
import { AccountTypeClassification } from "~/models/account";
import { IAccountTypeCreateRequest } from "~/models/accountType";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";

const AddAccountType = (): React.ReactNode => {
  const [isChildType, setIsChildType] = React.useState(false);

  const { t } = useTranslation();
  const { allAccountTypes } = useAccountTypes();
  const { request } = useAuth();

  const nameField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value.trim().length === 0 ? t("name_is_required") : null,
  });

  const parentField = useField<string>({
    initialValue: "",
  });

  const classificationField = useField<string>({
    initialValue: AccountTypeClassification.Asset,
  });

  const queryClient = useQueryClient();
  const doAddAccountType = useMutation({
    mutationFn: async (accountType: IAccountTypeCreateRequest) =>
      await request({
        url: "/api/accountType",
        method: "POST",
        data: accountType,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: [accountTypesQueryKey] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      nameField.reset();
      parentField.reset();
      classificationField.setValue(AccountTypeClassification.Asset);
      setIsChildType(false);
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  const parentTypes = allAccountTypes.filter((type) => type.parent === "");

  const getClassificationForSubmit = (): string => {
    if (isChildType) {
      const parent = allAccountTypes.find(
        (t) => t.value.toLowerCase() === parentField.getValue().toLowerCase(),
      );
      return parent?.classification ?? classificationField.getValue();
    }
    return classificationField.getValue();
  };

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doAddAccountType.isPending} />
      <Stack>
        <TextInput
          {...nameField.getInputProps()}
          label={<PrimaryText size="sm">{t("account_type_name")}</PrimaryText>}
          elevation={1}
        />
        <Stack gap="0.25rem" justify="center">
          <PrimaryText size="sm">{t("category_type")}</PrimaryText>
          <SegmentedControl
            color="var(--mantine-primary-color-filled)"
            radius="md"
            value={isChildType ? "child" : "parent"}
            onChange={(val) => {
              const child = val === "child";
              setIsChildType(child);
              if (!child) {
                parentField.reset();
              }
            }}
            data={[
              { label: t("parent"), value: "parent" },
              { label: t("child"), value: "child" },
            ]}
          />
        </Stack>
        {isChildType ? (
          <Stack gap="0.25rem">
            <PrimaryText size="sm">{t("parent_account_type")}</PrimaryText>
            <CategorySelect
              w="100%"
              categories={parentTypes}
              value={parentField.getValue()}
              onChange={(val: string) => parentField.setValue(val)}
              withinPortal
              elevation={1}
            />
          </Stack>
        ) : (
          <Stack gap="0.25rem">
            <PrimaryText size="sm">{t("classification")}</PrimaryText>
            <SegmentedControl
              color="var(--mantine-primary-color-filled)"
              radius="md"
              value={classificationField.getValue()}
              onChange={(val) => classificationField.setValue(val)}
              data={[
                { label: t("asset"), value: AccountTypeClassification.Asset },
                {
                  label: t("liability"),
                  value: AccountTypeClassification.Liability,
                },
              ]}
            />
          </Stack>
        )}
        <Button
          w="100%"
          onClick={() => {
            doAddAccountType.mutate({
              value: nameField.getValue(),
              parent: isChildType ? parentField.getValue() : "",
              classification: getClassificationForSubmit(),
            });
          }}
        >
          {t("add_account_type")}
        </Button>
      </Stack>
    </Card>
  );
};

export default AddAccountType;
