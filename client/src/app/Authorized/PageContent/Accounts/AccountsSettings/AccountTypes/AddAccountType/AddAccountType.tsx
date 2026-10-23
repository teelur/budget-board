import { Stack } from "@mantine/core";
import {
  BodyText,
  Button,
  Card,
  CategorySelect,
  SegmentedControl,
  TextInput,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import { AccountTypeClassification } from "~/models/account";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";
import { useCreateAccountTypeMutation } from "~/hooks/mutations/accountTypes/useCreateAccountTypeMutation";

const AddAccountType = (): React.ReactNode => {
  const [isChildType, setIsChildType] = React.useState(false);

  const { t } = useTranslation();
  const { allAccountTypes } = useAccountTypes();

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

  const createAccountTypeMutation = useCreateAccountTypeMutation();

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
    <Card>
      <Stack>
        <TextInput
          {...nameField.getInputProps()}
          label={t("account_type_name")}
        />
        <Stack gap="0.25rem" justify="center">
          <BodyText component="span" tone="primary" size="sm" fw={600}>
            {t("category_level")}
          </BodyText>
          <SegmentedControl
            color="secondary"
            size="sm"
            fullWidth
            value={isChildType ? "child" : "parent"}
            data={[
              { label: t("parent"), value: "parent" },
              { label: t("child"), value: "child" },
            ]}
            onChange={(val) => {
              const child = val === "child";
              setIsChildType(child);
              if (!child) {
                parentField.reset();
              }
            }}
            aria-label={t("category_level")}
          />
        </Stack>
        {isChildType ? (
          <CategorySelect
            w="100%"
            label={t("parent_account_type")}
            categories={parentTypes}
            value={parentField.getValue()}
            onChange={(val: string) => parentField.setValue(val)}
            withinPortal
          />
        ) : (
          <Stack gap="0.25rem">
            <BodyText component="span" tone="primary" size="sm" fw={600}>
              {t("classification")}
            </BodyText>
            <SegmentedControl
              color="secondary"
              size="sm"
              fullWidth
              value={classificationField.getValue()}
              data={[
                { label: t("asset"), value: AccountTypeClassification.Asset },
                {
                  label: t("liability"),
                  value: AccountTypeClassification.Liability,
                },
              ]}
              onChange={(val) => classificationField.setValue(val)}
              aria-label={t("classification")}
            />
          </Stack>
        )}
        <Button
          variant="filled"
          color="primary"
          size="xs"
          fullWidth
          loading={createAccountTypeMutation.isPending}
          onClick={() => {
            createAccountTypeMutation.mutate(
              {
                value: nameField.getValue(),
                parent: isChildType ? parentField.getValue() : "",
                classification: getClassificationForSubmit(),
              },
              {
                onSuccess: () => {
                  nameField.reset();
                  parentField.reset();
                  classificationField.setValue(AccountTypeClassification.Asset);
                  setIsChildType(false);
                },
              },
            );
          }}
        >
          {t("add_account_type")}
        </Button>
      </Stack>
    </Card>
  );
};

export default AddAccountType;
