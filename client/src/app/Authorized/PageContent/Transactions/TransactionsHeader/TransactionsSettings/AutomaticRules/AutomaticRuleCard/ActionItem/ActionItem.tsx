import { Badge, Group } from "@mantine/core";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { getFormattedValue } from "~/helpers/automaticRules";
import {
  ActionOperators,
  IRuleParameterResponse,
  TransactionFields,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface ActionItemProps {
  action: IRuleParameterResponse;
  categories: ICategory[];
  currency: string;
}

const ActionItem = (props: ActionItemProps) => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();

  const formatDate = (dateStr: string): string =>
    dayjs(dateStr).format(dateFormat);

  const getCardContent = (): React.ReactNode => {
    if (props.action.operator === "set") {
      return (
        <>
          <Badge bg="purple" size="sm">
            {TransactionFields.find(
              (field) => field.value === props.action.field,
            )?.label ?? props.action.field}
          </Badge>
          <PrimaryText size="sm">{t("to")}</PrimaryText>
          <Badge size="sm">
            {getFormattedValue(
              props.action.field,
              props.action.value,
              props.currency,
              props.categories,
              formatDate,
              intlLocale,
            )}
          </Badge>
        </>
      );
    }
    return null;
  };

  const operatorLabelKey = ActionOperators.find(
    (op) => op.value === props.action.operator,
  )?.label;

  return (
    <Card p="0.25rem" shadow="xs" elevation={1}>
      <Group gap="0.3rem">
        <PrimaryText size="sm">
          {operatorLabelKey ? t(operatorLabelKey) : props.action.operator}
        </PrimaryText>
        {getCardContent()}
      </Group>
    </Card>
  );
};

export default ActionItem;
