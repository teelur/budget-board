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
import { useAssetTypes } from "~/providers/AssetTypeProvider/AssetTypeProvider";
import { useCreateAssetTypeMutation } from "~/hooks/mutations/assetTypes/useCreateAssetTypeMutation";

const AddAssetType = (): React.ReactNode => {
  const [isChildType, setIsChildType] = React.useState(false);

  const { t } = useTranslation();
  const { allAssetTypes } = useAssetTypes();
  const createAssetTypeMutation = useCreateAssetTypeMutation();

  const nameField = useField<string>({
    initialValue: "",
    validate: (value) =>
      value.trim().length === 0 ? t("name_is_required") : null,
  });

  const parentField = useField<string>({
    initialValue: "",
  });

  const parentTypes = allAssetTypes.filter((type) => type.parent === "");

  return (
    <Card>
      <Stack gap="0.5rem">
        <TextInput
          {...nameField.getInputProps()}
          label={t("asset_type_name")}
        />
        <Stack gap="0.25rem" justify="center">
          <BodyText tone="primary" size="sm" fw={600}>
            {t("category_level")}
          </BodyText>
          <SegmentedControl
            color="secondary"
            size="compact-sm"
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
        {isChildType && (
          <CategorySelect
            w="100%"
            label={t("parent_asset_type")}
            categories={parentTypes}
            value={parentField.getValue()}
            onChange={(val: string) => parentField.setValue(val)}
            withinPortal
          />
        )}
        <Button
          variant="filled"
          color="primary"
          size="xs"
          fullWidth
          loading={createAssetTypeMutation.isPending}
          onClick={() => {
            createAssetTypeMutation.mutate(
              {
                value: nameField.getValue(),
                parent: isChildType ? parentField.getValue() : "",
              },
              {
                onSuccess: () => {
                  nameField.reset();
                  parentField.reset();
                  setIsChildType(false);
                },
              },
            );
          }}
        >
          {t("add_asset_type")}
        </Button>
      </Stack>
    </Card>
  );
};

export default AddAssetType;
