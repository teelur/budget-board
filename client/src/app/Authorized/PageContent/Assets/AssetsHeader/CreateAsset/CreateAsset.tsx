import { ActionIcon, Button, Stack } from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PlusIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IAssetCreateRequest } from "~/models/asset";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import BaseTextInput from "~/components/core/Input/Base/BaseTextInput/BaseTextInput";
import { useTranslation } from "react-i18next";

const CreateAsset = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const { t } = useTranslation();

  const assetNameField = useField<string>({
    initialValue: "",
    validate: isNotEmpty(t("name_is_required")),
  });

  const { request } = useAuth();
  const queryClient = useQueryClient();
  const doCreateAsset = useMutation({
    mutationFn: async (newAsset: IAssetCreateRequest) =>
      await request({
        url: "/api/asset",
        method: "POST",
        data: newAsset,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["assets"] });
      await queryClient.invalidateQueries({ queryKey: ["values"] });

      assetNameField.reset();
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
  });

  return (
    <>
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={opened}
        onClose={close}
        title={<PrimaryText>{t("create_asset")}</PrimaryText>}
      >
        <Stack gap="0.5rem">
          <BaseTextInput
            {...assetNameField.getInputProps()}
            label={<PrimaryText size="sm">{t("name")}</PrimaryText>}
            placeholder={t("enter_asset_name")}
          />
          <Button
            loading={doCreateAsset.isPending}
            onClick={() =>
              doCreateAsset.mutate({
                name: assetNameField.getValue(),
              } as IAssetCreateRequest)
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
