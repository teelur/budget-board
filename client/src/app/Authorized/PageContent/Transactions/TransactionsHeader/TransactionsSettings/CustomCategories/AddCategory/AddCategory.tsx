import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { Button, LoadingOverlay, Switch, Stack, Group } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { ICategoryCreateRequest } from "~/models/category";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import Card from "~/components/core/Card/Card";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import { useTranslation } from "react-i18next";

const AddCategory = (): React.ReactNode => {
  const [isChildCategory, setIsChildCategory] = React.useState(false);

  const { t } = useTranslation();
  const { transactionCategories } = useTransactionCategories();

  const nameField = useField<string>({
    initialValue: "",
    validate: (value) => (value.length === 0 ? t("name_is_required") : null),
  });
  const parentField = useField<string>({
    initialValue: "",
  });

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doAddCategory = useMutation({
    mutationFn: async (category: ICategoryCreateRequest) =>
      await request({
        url: "/api/transactionCategory",
        method: "POST",
        data: category,
      }),
    onSuccess: async () => {
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
    },
    onError: (error: AxiosError) =>
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      }),
  });

  const parentCategories = transactionCategories.filter(
    (category) => category.parent?.length === 0,
  );

  return (
    <Card elevation={2}>
      <LoadingOverlay visible={doAddCategory.isPending} />
      <Stack>
        <TextInput
          {...nameField.getInputProps()}
          label={<PrimaryText size="sm">{t("category_name")}</PrimaryText>}
          elevation={2}
        />
        <Stack gap="0.5rem" justify="center">
          <PrimaryText size="sm">{t("category_type")}</PrimaryText>
          <Group gap="0.5rem">
            <DimmedText size="sm">{t("parent")}</DimmedText>
            <Switch
              checked={isChildCategory}
              onChange={(event) => {
                setIsChildCategory(event.currentTarget.checked);
                if (!event.currentTarget.checked) {
                  parentField.setValue("");
                }
              }}
              size="md"
            />
            <DimmedText size="sm">{t("child")}</DimmedText>
          </Group>
        </Stack>
        {isChildCategory && (
          <Stack gap="0.25rem">
            <PrimaryText size="sm">{t("parent_category")}</PrimaryText>
            <CategorySelect
              w="100%"
              categories={parentCategories}
              {...parentField.getInputProps()}
              withinPortal
              elevation={2}
            />
          </Stack>
        )}
        <Button
          w="100%"
          onClick={() =>
            doAddCategory.mutate({
              value: nameField.getValue(),
              parent: parentField.getValue(),
            })
          }
        >
          {t("add_category")}
        </Button>
      </Stack>
    </Card>
  );
};

export default AddCategory;
