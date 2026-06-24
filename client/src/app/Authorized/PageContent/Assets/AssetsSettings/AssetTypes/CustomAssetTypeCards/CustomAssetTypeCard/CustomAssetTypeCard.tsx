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
import { useDeleteAssetTypeMutation } from "~/hooks/mutations/assetTypes/useDeleteAssetTypeMutation";
import { useUpdateAssetTypeMutation } from "~/hooks/mutations/assetTypes/useUpdateAssetTypeMutation";
import { IAssetTypeResponse } from "~/models/assetType";
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";

interface CustomAssetTypeCardProps {
  assetType: IAssetTypeResponse;
  isBuiltIn?: boolean;
  isChildCard?: boolean;
}

const CustomAssetTypeCard = (
  props: CustomAssetTypeCardProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const { allAssetTypes } = useAssetTypes();
  const updateAssetTypeMutation = useUpdateAssetTypeMutation();
  const deleteAssetTypeMutation = useDeleteAssetTypeMutation();

  const [isEditing, setIsEditing] = React.useState(false);
  const [isChildType, setIsChildType] = React.useState(
    props.assetType.parent !== "",
  );

  const nameField = useField<string>({
    initialValue: props.assetType.value,
    validate: (value) =>
      value.trim().length === 0 ? t("name_is_required") : null,
  });

  const parentField = useField<string>({
    initialValue: props.assetType.parent,
  });

  const parentTypes = allAssetTypes.filter((type) => type.parent === "");

  const handleCancel = () => {
    nameField.setValue(props.assetType.value);
    parentField.setValue(props.assetType.parent);
    setIsChildType(props.assetType.parent !== "");
    setIsEditing(false);
  };

  if (isEditing && !props.isBuiltIn) {
    return (
      <Group w="100%" maw={600} wrap="nowrap">
        {props.isChildCard && <CornerDownRight />}
        <Card flex={1} p="0.5rem" elevation={1}>
          <LoadingOverlay
            visible={
              updateAssetTypeMutation.isPending ||
              deleteAssetTypeMutation.isPending
            }
          />
          <Stack gap="0.5rem">
            <TextInput
              {...nameField.getInputProps()}
              label={
                <PrimaryText size="sm">{t("asset_type_name")}</PrimaryText>
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
            {isChildType && (
              <Stack gap="0.25rem">
                <PrimaryText size="sm">{t("parent_asset_type")}</PrimaryText>
                <CategorySelect
                  w="100%"
                  categories={parentTypes}
                  value={parentField.getValue()}
                  onChange={(val: string) => parentField.setValue(val)}
                  withinPortal
                  elevation={1}
                />
              </Stack>
            )}
            <Group justify="flex-end" gap="0.5rem">
              <Button variant="default" size="xs" onClick={handleCancel}>
                {t("cancel")}
              </Button>
              <Button
                size="xs"
                onClick={() =>
                  updateAssetTypeMutation.mutate(
                    {
                      id: props.assetType.id,
                      value: nameField.getValue(),
                      parent: isChildType ? parentField.getValue() : "",
                    },
                    {
                      onSuccess: () => {
                        setIsEditing(false);
                      },
                    },
                  )
                }
              >
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
              <DimmedText size="sm">{props.assetType.value}</DimmedText>
            ) : (
              <PrimaryText size="sm">{props.assetType.value}</PrimaryText>
            )}
            {props.isBuiltIn && <Badge size="xs">{t("built_in")}</Badge>}
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
                  onClick={() =>
                    deleteAssetTypeMutation.mutateAsync(props.assetType.id)
                  }
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

export default CustomAssetTypeCard;
