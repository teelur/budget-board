import { ActionIcon, Group, LoadingOverlay } from "@mantine/core";
import { useField } from "@mantine/form";
import { PencilIcon } from "lucide-react";
import { IInstitution, IInstitutionUpdateRequest } from "~/models/institution";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useUpdateInstitutionMutation } from "~/hooks/mutations/institutions/useUpdateInstitutionMutation";

interface IEditableInstitutionItemContentProps {
  institution: IInstitution;
  totalBalance: number;
  toggle: () => void;
}

const EditableInstitutionItemContent = (
  props: IEditableInstitutionItemContentProps,
) => {
  const institutionNameField = useField({
    initialValue: props.institution.name,
  });

  const updateInstitutionMutation = useUpdateInstitutionMutation();

  return (
    <Group justify="space-between" align="center" gap="0.5rem">
      <LoadingOverlay visible={updateInstitutionMutation.isPending} />
      <Group wrap="nowrap" gap="0.5rem">
        <TextInput
          w={250}
          maw="100%"
          {...institutionNameField.getInputProps()}
          onBlur={() =>
            updateInstitutionMutation.mutate({
              id: props.institution.id,
              name: institutionNameField.getValue(),
            } as IInstitutionUpdateRequest)
          }
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
        <SensitiveAmount amount={props.totalBalance} />
      </StatusText>
    </Group>
  );
};

export default EditableInstitutionItemContent;
