import { Group, LoadingOverlay } from "@mantine/core";
import { ActionIcon, AmountText, TextInput } from "@teelur/budget-board-ui";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useField } from "@mantine/form";
import { PencilIcon } from "lucide-react";
import { IInstitution, IInstitutionUpdateRequest } from "~/models/institution";
import { useUpdateInstitutionMutation } from "~/hooks/mutations/institutions/useUpdateInstitutionMutation";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

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

  const { isPrivacyModeEnabled } = usePrivacyMode();
  const updateInstitutionMutation = useUpdateInstitutionMutation();
  const { intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();

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
        />
        <ActionIcon
          variant="outline"
          color="primary"
          size="xs"
          onClick={(e) => {
            e.stopPropagation();
            props.toggle();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <AmountText
        amount={props.totalBalance}
        size="lg"
        isSensitive={isPrivacyModeEnabled}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={decimalPlaces}
      />
    </Group>
  );
};

export default EditableInstitutionItemContent;
