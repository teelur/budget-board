import { Group } from "@mantine/core";
import { Badge, BodyText } from "@teelur/budget-board-ui";
import { useTranslation } from "react-i18next";
import { getFormattedValue } from "~/helpers/automaticRules";
import { IAccountResponse } from "~/models/account";
import {
  ConditionTransactionFields,
  IRuleParameterResponse,
  Operators,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface ConditionItemProps {
  condition: IRuleParameterResponse;
  categories: ICategory[];
  accounts: IAccountResponse[];
  currency: string;
}

const ConditionItem = (props: ConditionItemProps) => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
  const { decimalPlaces } = useUserSettings();

  const fieldLabelKey = ConditionTransactionFields.find(
    (field) => field.value === props.condition.field,
  )?.label;

  const operatorLabelKey = Operators.find(
    (op) => op.value === props.condition.operator,
  )?.label;

  const formatDate = (dateStr: string): string =>
    dayjs(dateStr).format(dateFormat);

  return (
    <Group gap="0.3rem">
      <Badge variant="outline" color="secondary" size="xs">
        {fieldLabelKey ? t(fieldLabelKey) : props.condition.field}
      </Badge>
      <BodyText tone="primary" size="sm" fw={500}>
        {operatorLabelKey ? t(operatorLabelKey) : props.condition.operator}
      </BodyText>
      <Badge variant="filled" color="accent" size="xs">
        {getFormattedValue(
          props.condition.field,
          props.condition.value,
          props.currency,
          decimalPlaces,
          props.categories,
          formatDate,
          intlLocale,
          props.accounts,
        )}
      </Badge>
    </Group>
  );
};

export default ConditionItem;
