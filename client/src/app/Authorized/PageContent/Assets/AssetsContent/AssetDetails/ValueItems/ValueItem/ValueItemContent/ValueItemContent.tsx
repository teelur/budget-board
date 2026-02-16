import { ActionIcon, Group } from "@mantine/core";
import { PencilIcon } from "lucide-react";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IValueResponse } from "~/models/value";
import { useDate } from "~/providers/DateProvider/DateProvider";

interface ValueItemContentProps {
  value: IValueResponse;
  userCurrency: string;
  doSelect: () => void;
}

const ValueItemContent = (props: ValueItemContentProps): React.ReactNode => {
  const { dayjs, longDateFormat } = useDate();
  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <PrimaryText size="md">
          {dayjs(props.value.dateTime).format(longDateFormat)}
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
        {convertNumberToCurrency(props.value.amount, true, props.userCurrency)}
      </StatusText>
    </Group>
  );
};

export default ValueItemContent;
