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
import { AccountTypeClassification } from "~/models/account";
import {
  IAccountTypeResponse,
  IAccountTypeUpdateRequest,
} from "~/models/accountType";
import { useAccountTypes } from "~/providers/AccountTypeProvider/AccountTypeProvider";

interface CustomAccountTypeCardProps {
  accountType: IAccountTypeResponse;
  isBuiltIn?: boolean;
  isChildCard?: boolean;
  deleteAccountType: () => Promise<void>;
  updateAccountType: (req: IAccountTypeUpdateRequest) => Promise<void>;
}

const CustomAccountTypeCard = (
  props: CustomAccountTypeCardProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const { allAccountTypes } = useAccountTypes();

  const [isEditing, setIsEditing] = React.useState(false);
  const [isSaving, setIsSaving] = React.useState(false);
  const [isChildType, setIsChildType] = React.useState(
    props.accountType.parent !== "",
  );

  const nameField = useField<string>({
    initialValue: props.accountType.value,
    validate: (value) =>
      value.trim().length === 0 ? t("name_is_required") : null,
  });

  const parentField = useField<string>({
    initialValue: props.accountType.parent,
  });

  const classificationField = useField<string>({
    initialValue: props.accountType.classification,
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

  const handleSave = async () => {
    await nameField.validate();
    if (nameField.error) return;
    setIsSaving(true);
    try {
      await props.updateAccountType({
        id: props.accountType.id,
        value: nameField.getValue(),
        parent: isChildType ? parentField.getValue() : "",
        classification: getClassificationForSubmit(),
      });
      setIsEditing(false);
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    nameField.setValue(props.accountType.value);
    parentField.setValue(props.accountType.parent);
    classificationField.setValue(props.accountType.classification);
    setIsChildType(props.accountType.parent !== "");
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
              label={
                <PrimaryText size="sm">{t("account_type_name")}</PrimaryText>
              }
              elevation={1}
            />
            <Stack gap="0.25rem" justify="center">
              <PrimaryText size="sm">{t("category_level")}</PrimaryText>
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
                    {
                      label: t("asset"),
                      value: AccountTypeClassification.Asset,
                    },
                    {
                      label: t("liability"),
                      value: AccountTypeClassification.Liability,
                    },
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
              <DimmedText size="sm">{props.accountType.value}</DimmedText>
            ) : (
              <PrimaryText size="sm">{props.accountType.value}</PrimaryText>
            )}
            {props.isBuiltIn && <Badge size="xs">{t("built_in")}</Badge>}
            <Badge size="xs" variant="outline">
              {props.accountType.classification === "asset"
                ? t("asset")
                : t("liability")}
            </Badge>
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
                  onClick={props.deleteAccountType}
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

export default CustomAccountTypeCard;
