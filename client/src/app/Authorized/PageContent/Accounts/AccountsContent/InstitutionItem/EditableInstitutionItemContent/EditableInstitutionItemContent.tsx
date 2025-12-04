import { ActionIcon, Group, LoadingOverlay } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PencilIcon } from "lucide-react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { convertNumberToCurrency } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IInstitution, IInstitutionUpdateRequest } from "~/models/institution";
import SurfaceTextInput from "~/components/core/Input/Surface/SurfaceTextInput/SurfaceTextInput";
import StatusText from "~/components/core/Text/StatusText/StatusText";

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
      notifications.show({
        message: "Institution updated",
        color: "var(--button-color-confirm)",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
      institutionNameField.setValue(props.institution.name);
    },
  });

  return (
    <Group justify="space-between" align="center" gap="0.5rem">
      <LoadingOverlay visible={doUpdateInstitution.isPending} />
      <Group wrap="nowrap" gap="0.5rem">
        <SurfaceTextInput
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
      <StatusText amount={props.totalBalance} size="md">
        {convertNumberToCurrency(props.totalBalance, true, props.userCurrency)}
      </StatusText>
    </Group>
  );
};

export default EditableInstitutionItemContent;
