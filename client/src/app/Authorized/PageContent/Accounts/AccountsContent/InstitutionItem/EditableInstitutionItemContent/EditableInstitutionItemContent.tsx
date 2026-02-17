import { ActionIcon, Group, LoadingOverlay } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PencilIcon } from "lucide-react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { IInstitution, IInstitutionUpdateRequest } from "~/models/institution";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface IEditableInstitutionItemContentProps {
  institution: IInstitution;
  totalBalance: number;
  userCurrency: string;
  toggle: () => void;
}

const EditableInstitutionItemContent = (
  props: IEditableInstitutionItemContentProps,
) => {
  const institutionNameField = useField({
    initialValue: props.institution.name,
  });

  const { request } = useAuth();
  const { intlLocale } = useLocale();

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
        <TextInput
          w={250}
          maw="100%"
          {...institutionNameField.getInputProps()}
          onBlur={() => doUpdateInstitution.mutate()}
          elevation={1}
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
        {convertNumberToCurrency(
          props.totalBalance,
          true,
          props.userCurrency,
          SignDisplay.Auto,
          intlLocale,
        )}
      </StatusText>
    </Group>
  );
};

export default EditableInstitutionItemContent;
