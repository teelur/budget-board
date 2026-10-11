import { Group } from "@mantine/core";
import { ActionIcon, AmountText, BodyText } from "@teelur/budget-board-ui";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { PencilIcon } from "lucide-react";
import { IValueResponse } from "~/models/value";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface ValueItemContentProps {
  value: IValueResponse;
  doSelect: () => void;
}

const ValueItemContent = (props: ValueItemContentProps): React.ReactNode => {
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <BodyText tone="primary" size="sm" fw={500}>
          {dayjs(props.value.date).format(longDateFormat)}
        </BodyText>
        <ActionIcon
          variant="ghost"
          color="primary"
          size="compact-xs"
          onClick={(e) => {
            e.stopPropagation();
            props.doSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <AmountText
        amount={props.value.amount}
        size="md"
        isSensitive={isPrivacyModeEnabled}
        locale={intlLocale}
        currency={preferredCurrency}
        decimalPlaces={decimalPlaces}
      />
    </Group>
  );
};

export default ValueItemContent;
