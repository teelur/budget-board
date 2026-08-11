import { ActionIcon, Group } from "@mantine/core";
import { Trash2Icon } from "lucide-react";
import React from "react";
import {
  deserializeActionTags,
  getDefaultValue,
  serializeActionTags,
} from "~/helpers/automaticRules";
import {
  ActionTransactionFields,
  getActionOperators,
  IRuleParameterEdit,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";
import Card from "~/components/core/Card/Card";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import TransactionTagsInput from "~/components/TransactionTagsInput/TransactionTagsInput";
import { isValidAmountExpression } from "~/helpers/automaticRuleExpressions";

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
  const { dayjsLocale, longDateFormat } = useLocale();

  const setValue = (value: string) =>
    props.setRuleParameter({
      ...props.ruleParameter,
      value,
    });

  const getValueInput = (): React.ReactNode => {
    if (props.ruleParameter.field === "merchant") {
      return (
        <TextInput
          flex="1 1 auto"
          placeholder={t("enter_merchant_name")}
          value={props.ruleParameter.value}
          onChange={(event) => setValue(event.currentTarget.value)}
          elevation={1}
        />
      );
    } else if (props.ruleParameter.field === "amount") {
      return (
        <TextInput
          flex="1 1 auto"
          placeholder={t("enter_amount_expression")}
          value={props.ruleParameter.value}
          onChange={(event) => setValue(event.currentTarget.value)}
          error={
            !isValidAmountExpression(props.ruleParameter.value)
              ? t("invalid_amount_expression")
              : undefined
          }
          elevation={1}
        />
      );
    } else if (props.ruleParameter.field === "note") {
      return (
        <TextInput
          flex="1 1 auto"
          placeholder={t("enter_note")}
          value={props.ruleParameter.value}
          onChange={(event) => setValue(event.currentTarget.value)}
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
          onChange={(value) => setValue(value ?? "")}
          elevation={1}
        />
      );
    } else if (props.ruleParameter.field === "category") {
      return (
        <CategorySelect
          flex="1 1 auto"
          value={props.ruleParameter.value}
          onChange={setValue}
          categories={props.categories}
          withinPortal
          elevation={1}
        />
      );
    } else if (props.ruleParameter.field === "tags") {
      return (
        <TransactionTagsInput
          flex="1 1 auto"
          placeholder={t("select_tags")}
          value={deserializeActionTags(props.ruleParameter.value)}
          onChange={(tags) => setValue(serializeActionTags(tags))}
          error={
            deserializeActionTags(props.ruleParameter.value).length === 0
              ? t("at_least_one_tag_required")
              : undefined
          }
          elevation={1}
        />
      );
    }

    return null;
  };

  const getFieldSelect = (): React.ReactNode => (
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
        const foundField = ActionTransactionFields.find(
          (field) => field.value === value,
        );

        if (!foundField) {
          return;
        }

        props.setRuleParameter({
          ...props.ruleParameter,
          field: foundField.value,
          operator: getActionOperators(foundField.value)[0]?.value ?? "set",
          value: getDefaultValue(foundField.value),
        });
      }}
      elevation={1}
    />
  );

  const getOperatorSelect = (): React.ReactNode => (
    <Select
      data={getActionOperators(props.ruleParameter.field).map((op) => ({
        value: op.value,
        label: t(op.label),
      }))}
      value={
        getActionOperators(props.ruleParameter.field).find(
          (op) => op.value === props.ruleParameter.operator,
        )?.value
      }
      onChange={(value) => {
        const foundOperator = getActionOperators(
          props.ruleParameter.field,
        ).find((op) => op.value === value);

        if (!foundOperator) {
          return;
        }

        const isTagOperator = ["add", "remove"].includes(foundOperator.value);
        const nextField = isTagOperator
          ? "tags"
          : props.ruleParameter.field === "tags"
            ? "merchant"
            : props.ruleParameter.field;

        props.setRuleParameter({
          ...props.ruleParameter,
          field: nextField,
          operator: foundOperator.value,
          value:
            foundOperator.value === "delete"
              ? ""
              : isTagOperator
                ? props.ruleParameter.field === "tags"
                  ? props.ruleParameter.value
                  : getDefaultValue("tags")
                : props.ruleParameter.field === "tags"
                  ? getDefaultValue(nextField)
                  : props.ruleParameter.value,
        });
      }}
      elevation={1}
    />
  );

  return (
    <Card elevation={1}>
      <Group gap="0.5rem">
        {props.ruleParameter.operator === "delete" ? (
          getOperatorSelect()
        ) : (
          <>
            {getFieldSelect()}
            {getOperatorSelect()}
            <PrimaryText size="sm">{t("to")}</PrimaryText>
            {getValueInput()}
          </>
        )}
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
