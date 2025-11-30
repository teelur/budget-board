import { ActionIcon, Group } from "@mantine/core";
import dayjs from "dayjs";
import { PencilIcon } from "lucide-react";
import StatusText from "~/components/Text/StatusText/StatusText";
import SurfacePrimaryText from "~/components/Text/Surface/SurfacePrimaryText/SurfacePrimaryText";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IValueResponse } from "~/models/value";

interface ValueItemContentProps {
  value: IValueResponse;
  userCurrency: string;
  doSelect: () => void;
}

const ValueItemContent = (props: ValueItemContentProps): React.ReactNode => {
  return (
    <Group justify="space-between" align="center">
      <Group gap="0.5rem">
        <SurfacePrimaryText size="md">
          {dayjs(props.value.dateTime).format("L")}
        </SurfacePrimaryText>
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
      <StatusText value={props.value.amount} size="md">
        {convertNumberToCurrency(props.value.amount, true, props.userCurrency)}
      </StatusText>
    </Group>
  );
};

export default ValueItemContent;
