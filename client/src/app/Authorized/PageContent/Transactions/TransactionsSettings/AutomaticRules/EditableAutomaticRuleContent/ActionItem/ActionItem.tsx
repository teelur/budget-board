import { ActionIcon, Group } from "@mantine/core";
import { Trash2Icon } from "lucide-react";
import React from "react";
import { getDefaultValue } from "~/helpers/automaticRules";
import { getCurrencySymbol } from "~/helpers/currency";
import {
  ActionTransactionFields,
  ActionOperators,
  IRuleParameterEdit,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";
import Card from "~/components/core/Card/Card";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

export interface ActionItemProps {
  ruleParameter: IRuleParameterEdit;
  setRuleParameter: (newParameter: IRuleParameterEdit) => void;
  allowDelete: boolean;
  doDelete: (index: number) => void;
  index: number;
  categories: ICategory[];
}

const ActionItem = (props: ActionItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjsLocale, longDateFormat, thousandsSeparator, decimalSeparator } =
    useLocale();
  const { preferredCurrency } = useUserSettings();

  const getValueInput = (): React.ReactNode => {
    if (props.ruleParameter.field === "merchant") {
      return (
        <TextInput
          flex="1 1 auto"
          placeholder={t("enter_merchant_name")}
          value={props.ruleParameter.value}
          onChange={(event) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value: event.currentTarget.value,
            })
          }
          elevation={1}
        />
      );
    } else if (props.ruleParameter.field === "amount") {
      return (
        <NumberInput
          flex="1 1 auto"
          placeholder={t("enter_amount")}
          value={props.ruleParameter.value}
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value: (typeof value === "number" ? value : 0).toString(),
            })
          }
          prefix={getCurrencySymbol(preferredCurrency)}
          decimalScale={2}
          thousandSeparator={thousandsSeparator}
          decimalSeparator={decimalSeparator}
          elevation={1}
        />
      );
    } else if (props.ruleParameter.field === "date") {
      return (
        <DateInput
          flex="1 1 auto"
          placeholder={t("select_a_date")}
          value={props.ruleParameter.value}
          locale={dayjsLocale}
          valueFormat={longDateFormat}
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value: value ?? "",
            })
          }
          elevation={1}
        />
      );
    } else if (props.ruleParameter.field === "category") {
      return (
        <CategorySelect
          flex="1 1 auto"
          value={props.ruleParameter.value}
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value,
            })
          }
          categories={props.categories}
          withinPortal
          elevation={1}
        />
      );
    }

    return null;
  };

  const getCardContent = (): React.ReactNode => {
    if (props.ruleParameter.operator === "set") {
      return (
        <>
          <Select
            data={ActionTransactionFields.map((i) => ({
              ...i,
              label: t(i.label),
            }))}
            value={
              ActionTransactionFields.find(
                (field) => field.value === props.ruleParameter.field,
              )?.value
            }
            onChange={(value) => {
              const foundValue = ActionTransactionFields.find(
                (field) => field.value === value,
              );

              if (!foundValue) {
                return;
              }

              props.setRuleParameter({
                ...props.ruleParameter,
                field: foundValue.value,
                value: getDefaultValue(foundValue.value),
              });
            }}
            elevation={1}
          />
          <PrimaryText size="sm">{t("to")}</PrimaryText>
          {getValueInput()}
        </>
      );
    }
    return null;
  };

  return (
    <Card elevation={1}>
      <Group gap="0.5rem">
        <Select
          data={ActionOperators.map((op) => ({
            value: op.value,
            label: t(op.label),
          }))}
          value={
            ActionOperators.find(
              (op) => op.value === props.ruleParameter.operator,
            )?.value
          }
          onChange={(value) => {
            props.setRuleParameter({
              ...props.ruleParameter,
              operator:
                ActionOperators.find((op) => op.value === value)?.value ??
                ActionOperators[0]!.value,
            });
          }}
          elevation={1}
        />
        {getCardContent()}
        {props.allowDelete && (
          <Group style={{ alignSelf: "stretch" }}>
            <ActionIcon
              h="100%"
              size="sm"
              color="var(--button-color-destructive)"
              onClick={() => props.doDelete(props.index)}
            >
              <Trash2Icon size={16} />
            </ActionIcon>
          </Group>
        )}
      </Group>
    </Card>
  );
};

export default ActionItem;
