import { Group } from "@mantine/core";
import { Badge } from "@teelur/budget-board-ui";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import {
  deserializeActionTags,
  getFormattedValue,
} from "~/helpers/automaticRules";
import {
  ActionTransactionFields,
  ActionOperators,
  IRuleParameterResponse,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface ActionItemProps {
  action: IRuleParameterResponse;
  categories: ICategory[];
  currency: string;
}

const ActionItem = (props: ActionItemProps) => {
  const { t } = useTranslation();
  const { dayjs, dateFormat, intlLocale } = useLocale();
  const { decimalPlaces } = useUserSettings();

  const formatDate = (dateStr: string): string =>
    dayjs(dateStr).format(dateFormat);

  const getCardContent = (): React.ReactNode => {
    if (props.action.operator !== "delete") {
      const tagValues =
        props.action.field === "tags"
          ? deserializeActionTags(props.action.value)
          : [];

      return (
        <>
          <Badge variant="outline" color="secondary" size="xs">
            {t(
              ActionTransactionFields.find(
                (field) => field.value === props.action.field,
              )?.label ?? props.action.field,
            )}
          </Badge>
          <PrimaryText size="sm">{t("to")}</PrimaryText>
          {tagValues.length > 0 ? (
            <Group gap="0.25rem">
              {tagValues.map((tag) => (
                <Badge key={tag} variant="filled" color="primary" size="xs">
                  {tag}
                </Badge>
              ))}
            </Group>
          ) : (
            <Badge size="sm">
              {getFormattedValue(
                props.action.field,
                props.action.value,
                props.currency,
                decimalPlaces,
                props.categories,
                formatDate,
                intlLocale,
              )}
            </Badge>
          )}
        </>
      );
    }
    return null;
  };

  const operatorLabelKey = ActionOperators.find(
    (op) => op.value === props.action.operator,
  )?.label;

  return (
    <Card p="0.25rem" shadow="xs" elevation={2}>
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
