import { Badge, Group } from "@mantine/core";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { getFormattedValue } from "~/helpers/automaticRules";
import {
  IRuleParameterResponse,
  Operators,
  TransactionFields,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface ConditionItemProps {
  condition: IRuleParameterResponse;
  categories: ICategory[];
  currency: string;
}

const ConditionItem = (props: ConditionItemProps) => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();

  const fieldLabelKey = TransactionFields.find(
    (field) => field.value === props.condition.field,
  )?.label;

  const operatorLabelKey = Operators.find(
    (op) => op.value === props.condition.operator,
  )?.label;

  const formatDate = (dateStr: string): string =>
    dayjs(dateStr).format(dateFormat);

  return (
    <Card p="0.25rem" shadow="xs" elevation={1}>
      <Group gap="0.3rem">
        <Badge bg="purple" size="sm">
          {fieldLabelKey ? t(fieldLabelKey) : props.condition.field}
        </Badge>
        <PrimaryText size="sm">
          {operatorLabelKey ? t(operatorLabelKey) : props.condition.operator}
        </PrimaryText>
        <Badge size="sm">
          {getFormattedValue(
            props.condition.field,
            props.condition.value,
            props.currency,
            props.categories,
            formatDate,
            intlLocale,
          )}
        </Badge>
      </Group>
    </Card>
  );
};

export default ConditionItem;
