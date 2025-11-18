import {
  ActionIcon,
  Group,
  LoadingOverlay,
  Text,
  TextInput,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PencilIcon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { convertNumberToCurrency } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IInstitution, IInstitutionUpdateRequest } from "~/models/institution";

interface IEditableInstitutionItemContentProps {
  institution: IInstitution;
  totalBalance: number;
  userCurrency: string;
  toggle: () => void;
}

const EditableInstitutionItemContent = (
  props: IEditableInstitutionItemContentProps
) => {
  const institutionNameField = useField({
    initialValue: props.institution.name,
  });

  const { request } = useAuth();

  const queryClient = useQueryClient();
  const doUpdateInstitution = useMutation({
    mutationFn: async () => {
      const editedInstitution: IInstitutionUpdateRequest = {
        id: props.institution.id,
        name: institutionNameField.getValue(),
      };

      return await request({
        url: "/api/institution",
        method: "PUT",
        data: editedInstitution,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
      notifications.show({ message: "Institution updated", color: "green" });
    },
    onError: (error: AxiosError) => {
      notifications.show({ color: "red", message: translateAxiosError(error) });
      institutionNameField.setValue(props.institution.name);
    },
  });

  return (
    <Group justify="space-between" align="center" gap="0.5rem">
      <LoadingOverlay visible={doUpdateInstitution.isPending} />
      <Group wrap="nowrap" gap="0.5rem">
        <TextInput
          w={250}
          maw="100%"
          {...institutionNameField.getInputProps()}
          onBlur={() => doUpdateInstitution.mutate()}
        />
        <ActionIcon
          variant="outline"
          size="md"
          onClick={(e) => {
            e.stopPropagation();
            props.toggle();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <Text fw={600} size="md" c={props.totalBalance < 0 ? "red" : "green"}>
        {convertNumberToCurrency(props.totalBalance, true, props.userCurrency)}
      </Text>
    </Group>
  );
};

export default EditableInstitutionItemContent;
