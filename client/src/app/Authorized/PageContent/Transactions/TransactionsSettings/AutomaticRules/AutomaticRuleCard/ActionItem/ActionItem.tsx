import { Group } from "@mantine/core";
import { Badge, BodyText } from "@teelur/budget-board-ui";
import { useTranslation } from "react-i18next";
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
          <BodyText component="span" tone="primary" size="sm" fw={500}>
            {t("to")}
          </BodyText>
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
    <Group gap="0.3rem">
      <BodyText component="span" tone="primary" size="sm" fw={500}>
        {operatorLabelKey ? t(operatorLabelKey) : props.action.operator}
      </BodyText>
      {getCardContent()}
    </Group>
  );
};

export default ActionItem;
