import { Button, LoadingOverlay, SegmentedControl, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
    <Card elevation={1}>
      <LoadingOverlay visible={createAssetTypeMutation.isPending} />
      <Stack>
        <TextInput
          {...nameField.getInputProps()}
          label={<PrimaryText size="sm">{t("asset_type_name")}</PrimaryText>}
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
        <Button
          w="100%"
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
