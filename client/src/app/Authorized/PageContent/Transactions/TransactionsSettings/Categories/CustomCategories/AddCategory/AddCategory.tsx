import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import {
  transactionCategoriesQueryKey,
  translateAxiosError,
} from "~/helpers/requests";
import { Button, LoadingOverlay, SegmentedControl, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { CategoryTypes, ICategoryCreateRequest } from "~/models/category";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Card from "~/components/core/Card/Card";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import { useTranslation } from "react-i18next";
import { useCreateTransactionCategoryMutation } from "~/hooks/mutations/transactionCategories/useCreateTransactionCategoryMutation";

const AddCategory = (): React.ReactNode => {
  const [isChildCategory, setIsChildCategory] = React.useState(false);

  const { t } = useTranslation();
  const { allTransactionCategories } = useTransactionCategories();
  const createTransactionCategoryMutation =
    useCreateTransactionCategoryMutation();

  const nameField = useField<string>({
    initialValue: "",
    validate: (value) => (value.length === 0 ? t("name_is_required") : null),
  });
  const parentField = useField<string>({
    initialValue: "",
  });
  const categoryTypeField = useField<string>({
    initialValue: CategoryTypes.Expense,
  });

  const parentCategories = allTransactionCategories.filter(
    (category) => category.parent?.length === 0,
  );

  const getCategoryTypeForSubmit = (): string => {
    if (isChildCategory) {
      const parent = allTransactionCategories.find(
        (c) => c.value.toLowerCase() === parentField.getValue().toLowerCase(),
      );
      return parent?.categoryType ?? categoryTypeField.getValue();
    }
    return categoryTypeField.getValue();
  };

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={createTransactionCategoryMutation.isPending} />
      <Stack>
        <TextInput
          {...nameField.getInputProps()}
          label={<PrimaryText size="sm">{t("category_name")}</PrimaryText>}
          elevation={1}
        />
        <Stack gap="0.25rem" justify="center">
          <PrimaryText size="sm">{t("category_level")}</PrimaryText>
          <SegmentedControl
            color="var(--mantine-primary-color-filled)"
            radius="md"
            value={isChildCategory ? "child" : "parent"}
            onChange={(val) => {
              const child = val === "child";
              setIsChildCategory(child);
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
        {isChildCategory ? (
          <Stack gap="0.25rem">
            <PrimaryText size="sm">{t("parent_category")}</PrimaryText>
            <CategorySelect
              w="100%"
              categories={parentCategories}
              value={parentField.getValue()}
              onChange={(val: string) => parentField.setValue(val)}
              withinPortal
              elevation={1}
            />
          </Stack>
        ) : (
          <Stack gap="0.25rem">
            <PrimaryText size="sm">{t("category_type")}</PrimaryText>
            <SegmentedControl
              color="var(--mantine-primary-color-filled)"
              radius="md"
              value={categoryTypeField.getValue()}
              onChange={(val) => categoryTypeField.setValue(val)}
              data={[
                { label: t("expense"), value: CategoryTypes.Expense },
                { label: t("income"), value: CategoryTypes.Income },
              ]}
            />
          </Stack>
        )}
        <Button
          w="100%"
          onClick={() =>
            createTransactionCategoryMutation.mutate(
              {
                value: nameField.getValue(),
                parent: parentField.getValue(),
                categoryType: getCategoryTypeForSubmit(),
              } as ICategoryCreateRequest,
              {
                onSuccess: () => {
                  nameField.reset();
                  parentField.reset();
                  categoryTypeField.setValue(CategoryTypes.Expense);
                  setIsChildCategory(false);
                },
              },
            )
          }
        >
          {t("add_category")}
        </Button>
      </Stack>
    </Card>
  );
};

export default AddCategory;
