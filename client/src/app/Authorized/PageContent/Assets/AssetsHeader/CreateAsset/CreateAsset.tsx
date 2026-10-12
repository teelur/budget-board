import { Stack } from "@mantine/core";
import { Button, Modal, TextInput } from "@teelur/budget-board-ui";
import { isNotEmpty, useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { PlusIcon } from "lucide-react";
import React from "react";
import { IAssetCreateRequest } from "~/models/asset";
import { useTranslation } from "react-i18next";
import { useCreateAssetMutation } from "~/hooks/mutations/assets/useCreateAssetMutation";

const CreateAsset = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();
  const createAssetMutation = useCreateAssetMutation();

  const assetNameField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("name_is_required")),
  });

  return (
    <>
      <Button variant="filled" color="primary" size="compact-sm" onClick={open}>
        <PlusIcon size={22} />
      </Button>
      <Modal opened={opened} onClose={close} title={t("create_asset")}>
        <Stack gap="0.5rem">
          <TextInput
            {...assetNameField.getInputProps()}
            label={t("name")}
            placeholder={t("enter_asset_name")}
          />
          <Button
            variant="filled"
            color="primary"
            size="compact-sm"
            loading={createAssetMutation.isPending}
            onClick={() =>
              createAssetMutation.mutate(
                {
                  name: assetNameField.getValue(),
                } as IAssetCreateRequest,
                {
                  onSuccess: () => {
                    assetNameField.reset();
                    close();
                  },
                },
              )
            }
          >
            {t("submit")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default CreateAsset;
