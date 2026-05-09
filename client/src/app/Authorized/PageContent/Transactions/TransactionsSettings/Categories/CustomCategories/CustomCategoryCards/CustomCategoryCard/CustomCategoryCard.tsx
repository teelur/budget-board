import {
  ActionIcon,
  Badge,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  SegmentedControl,
  Stack,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { CornerDownRight, PencilIcon, TrashIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import {
  CategoryTypes,
  ICategoryResponse,
  ICategoryUpdateRequest,
} from "~/models/category";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";

interface CustomCategoryCardProps {
  category: ICategoryResponse;
  isBuiltIn?: boolean;
  isChildCard?: boolean;
  deleteCategory: () => Promise<void>;
  updateCategory: (req: ICategoryUpdateRequest) => Promise<void>;
}

const CustomCategoryCard = (
  props: CustomCategoryCardProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const { allTransactionCategories } = useTransactionCategories();

  const [isEditing, setIsEditing] = React.useState(false);
  const [isSaving, setIsSaving] = React.useState(false);
  const [isChildCategory, setIsChildCategory] = React.useState(
    props.category.parent !== "",
  );

  const nameField = useField<string>({
    initialValue: props.category.value,
    validate: (value) =>
      value.trim().length === 0 ? t("name_is_required") : null,
  });

  const parentField = useField<string>({
    initialValue: props.category.parent,
  });

  const categoryTypeField = useField<string>({
    initialValue: props.category.categoryType,
  });

  const parentCategories = allTransactionCategories.filter(
    (c) => c.parent === "",
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

  const handleSave = async () => {
    await nameField.validate();
    if (nameField.error) return;
    setIsSaving(true);
    try {
      await props.updateCategory({
        id: props.category.id,
        value: nameField.getValue(),
        parent: isChildCategory ? parentField.getValue() : "",
        categoryType: getCategoryTypeForSubmit(),
      });
      setIsEditing(false);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    nameField.setValue(props.category.value);
    parentField.setValue(props.category.parent);
    categoryTypeField.setValue(props.category.categoryType);
    setIsChildCategory(props.category.parent !== "");
    setIsEditing(false);
  };

  if (isEditing && !props.isBuiltIn) {
    return (
      <Group w="100%" maw={600} wrap="nowrap">
        {props.isChildCard && <CornerDownRight />}
        <Card flex={1} p="0.5rem" elevation={1}>
          <LoadingOverlay visible={isSaving} />
          <Stack gap="0.5rem">
            <TextInput
              {...nameField.getInputProps()}
              label={<PrimaryText size="sm">{t("category_name")}</PrimaryText>}
              elevation={1}
            />
            <Stack gap="0.25rem" justify="center">
              <PrimaryText size="sm">{t("category_type")}</PrimaryText>
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
                <PrimaryText size="sm">{t("classification")}</PrimaryText>
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
            <Group justify="flex-end" gap="0.5rem">
              <Button variant="default" size="xs" onClick={handleCancel}>
                {t("cancel")}
              </Button>
              <Button size="xs" onClick={handleSave}>
                {t("save")}
              </Button>
            </Group>
          </Stack>
        </Card>
      </Group>
    );
  }

  return (
    <Group w="100%" maw={600} wrap="nowrap">
      {props.isChildCard && <CornerDownRight />}
      <Card flex={1} p="0.25rem" elevation={1}>
        <Group justify="space-between">
          <Group gap="0.5rem">
            {props.isBuiltIn ? (
              <DimmedText size="sm">{props.category.value}</DimmedText>
            ) : (
              <PrimaryText size="sm">{props.category.value}</PrimaryText>
            )}
            {props.isBuiltIn && <Badge size="xs">{t("built_in")}</Badge>}
            {!props.isChildCard && (
              <Badge size="xs" variant="outline">
                {props.category.categoryType === CategoryTypes.Income
                  ? t("income")
                  : t("expense")}
              </Badge>
            )}
          </Group>
          <Flex justify="flex-end" flex="1 1 auto" gap="0.25rem">
            {!props.isBuiltIn && (
              <>
                <ActionIcon
                  size="sm"
                  variant="subtle"
                  onClick={() => setIsEditing(true)}
                >
                  <PencilIcon size="1rem" />
                </ActionIcon>
                <ActionIcon
                  size="sm"
                  onClick={props.deleteCategory}
                  bg="var(--button-color-destructive)"
                >
                  <TrashIcon size="1rem" />
                </ActionIcon>
              </>
            )}
          </Flex>
        </Group>
      </Card>
    </Group>
  );
};

export default CustomCategoryCard;
