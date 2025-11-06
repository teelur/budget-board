import {
  ActionIcon,
  Button,
  Modal,
  Stack,
  Text,
  TextInput,
} from "@mantine/core";
import { isNotEmpty, useField } from "@mantine/form";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PlusIcon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IAssetCreateRequest } from "~/models/asset";

const CreateAsset = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);

  const assetNameField = useField<string>({
    initialValue: "",
    validate: isNotEmpty("Name is required"),
  });

  const { request } = React.useContext<any>(AuthContext);
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

      notifications.show({ message: "Asset created", color: "green" });

      assetNameField.reset();
    },
    onError: (error: AxiosError) => {
      notifications.show({ message: translateAxiosError(error), color: "red" });
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
        title={<Text fw={600}>Create Asset</Text>}
        styles={{
          inner: {
            left: "0",
            right: "0",
            padding: "0 !important",
          },
        }}
      >
        <Stack gap={10}>
          <TextInput {...assetNameField.getInputProps()} label="Asset Name" />
          <Button
            type="submit"
            loading={doCreateAsset.isPending}
            onClick={() =>
              doCreateAsset.mutate({
                name: assetNameField.getValue(),
              } as IAssetCreateRequest)
            }
          >
            Submit
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default CreateAsset;
