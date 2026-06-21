import { ActionIcon, Button, Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { PlusIcon } from "lucide-react";
import React from "react";
import { IAssetCreateRequest } from "~/models/asset";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
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
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={opened}
        onClose={close}
        title={
          <PrimaryHeading component="span" order={4}>
            {t("create_asset")}
          </PrimaryHeading>
        }
      >
        <Stack gap="0.5rem">
          <TextInput
            {...assetNameField.getInputProps()}
            label={<PrimaryText size="sm">{t("name")}</PrimaryText>}
            placeholder={t("enter_asset_name")}
            elevation={0}
          />
          <Button
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
