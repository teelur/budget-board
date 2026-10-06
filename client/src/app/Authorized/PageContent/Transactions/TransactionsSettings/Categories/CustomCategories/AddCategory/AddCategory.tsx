import { LoadingOverlay, Stack } from "@mantine/core";
import {
  Button,
  CategorySelect,
  SegmentedControl,
  TextInput,
} from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import { CategoryTypes, ICategoryCreateRequest } from "~/models/category";
import React from "react";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
        <TextInput {...nameField.getInputProps()} label={t("category_name")} />
        <Stack gap="0.25rem" justify="center">
          <PrimaryText size="sm">{t("category_level")}</PrimaryText>
          <SegmentedControl
            color="secondary"
            size="compact-sm"
            fullWidth
            value={isChildCategory ? "child" : "parent"}
            data={[
              { label: t("parent"), value: "parent" },
              { label: t("child"), value: "child" },
            ]}
            onChange={(val) => {
              const child = val === "child";
              setIsChildCategory(child);
              if (!child) {
                parentField.reset();
              }
            }}
            aria-label={t("category_level")}
          />
        </Stack>
        {isChildCategory ? (
          <CategorySelect
            w="100%"
            label={t("parent_category")}
            categories={parentCategories}
            value={parentField.getValue()}
            onChange={(val: string) => parentField.setValue(val)}
            withinPortal
          />
        ) : (
          <Stack gap="0.25rem">
            <PrimaryText size="sm">{t("category_type")}</PrimaryText>
            <SegmentedControl
              color="secondary"
              size="compact-sm"
              fullWidth
              value={categoryTypeField.getValue()}
              data={[
                { label: t("expense"), value: CategoryTypes.Expense },
                { label: t("income"), value: CategoryTypes.Income },
              ]}
              onChange={(val) => categoryTypeField.setValue(val)}
              aria-label={t("category_type")}
            />
          </Stack>
        )}
        <Button
          variant="filled"
          color="primary"
          size="xs"
          fullWidth
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
