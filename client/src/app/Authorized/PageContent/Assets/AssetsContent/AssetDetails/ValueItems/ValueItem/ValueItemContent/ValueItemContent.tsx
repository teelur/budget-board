import { ActionIcon, Group } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { maskedAmountText } from "~/helpers/privacy";
import { IValueResponse } from "~/models/value";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface ValueItemContentProps {
  value: IValueResponse;
  userCurrency: string;
  doSelect: () => void;
}

const ValueItemContent = (props: ValueItemContentProps): React.ReactNode => {
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { isPrivacyModeEnabled } = usePrivacyMode();
  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <PrimaryText size="md">
          {dayjs(props.value.date).format(longDateFormat)}
        </PrimaryText>
        <ActionIcon
          variant="transparent"
          size="md"
          onClick={(e) => {
            e.stopPropagation();
            props.doSelect();
          }}
        >
          <PencilIcon size={16} />
        </ActionIcon>
      </Group>
      <StatusText amount={props.value.amount} size="md">
        {isPrivacyModeEnabled
          ? maskedAmountText
          : convertNumberToCurrency(
              props.value.amount,
              true,
              props.userCurrency,
              SignDisplay.Auto,
              intlLocale,
            )}
      </StatusText>
    </Group>
  );
};

export default ValueItemContent;
