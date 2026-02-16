import { ActionIcon, ComboboxItem, Group } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { Trash2Icon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getDefaultValue } from "~/helpers/automaticRules";
import { getCurrencySymbol } from "~/helpers/currency";
import {
  FieldToOperatorType,
  IRuleParameterEdit,
  Operators,
  OperatorTypes,
  TransactionFields,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import { IUserSettings } from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import Select from "~/components/core/Select/Select/Select";
import { useTranslation } from "react-i18next";
import { useDate } from "~/providers/DateProvider/DateProvider";

export interface ConditionItemProps {
  ruleParameter: IRuleParameterEdit;
  setRuleParameter: (newParameter: IRuleParameterEdit) => void;
  allowDelete?: boolean;
  doDelete?: (index: number) => void;
  index: number;
  categories: ICategory[];
}

const ConditionItem = (props: ConditionItemProps): React.ReactNode => {
  const { t } = useTranslation();
  const { locale, longDateFormat } = useDate();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

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
          elevation={2}
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
          prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
          decimalScale={2}
          thousandSeparator=","
          elevation={2}
        />
      );
    } else if (props.ruleParameter.field === "date") {
      return (
        <DateInput
          flex="1 1 auto"
          placeholder={t("select_a_date")}
          value={props.ruleParameter.value}
          locale={locale}
          valueFormat={longDateFormat}
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value: value ?? "",
            })
          }
          elevation={2}
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
          elevation={2}
        />
      );
    }

    return null;
  };

  return (
    <Card elevation={2}>
      <Group gap="0.5rem">
        <Select
          data={TransactionFields.map((i) => ({
            ...i,
            label: t(i.label),
          }))}
          value={
            TransactionFields.find(
              (field) => field.value === props.ruleParameter.field,
            )?.value
          }
          onChange={(value) => {
            const foundField = TransactionFields.find(
              (field) => field.value === value,
            );

            if (!foundField) {
              return;
            }

            props.setRuleParameter({
              ...props.ruleParameter,
              field: foundField.value,
              operator:
                Operators.filter((op) =>
                  op.type.includes(
                    FieldToOperatorType.get(foundField.value) ??
                      OperatorTypes.STRING,
                  ),
                ).at(0)?.value ?? "",
              value: getDefaultValue(foundField.value),
            } as IRuleParameterEdit);
          }}
          allowDeselect={false}
          elevation={2}
        />
        <Select
          data={Operators.filter((op) =>
            op.type.includes(
              FieldToOperatorType.get(props.ruleParameter.field) ??
                OperatorTypes.STRING,
            ),
          ).map(
            (op) =>
              ({
                value: op.value,
                label: t(op.label),
              }) as ComboboxItem,
          )}
          value={
            Operators.find((op) => op.value === props.ruleParameter.operator)
              ?.value ?? ""
          }
          onChange={(value) => {
            const foundValue = Operators.find((op) => op.value === value);

            if (!foundValue) {
              return;
            }

            props.setRuleParameter({
              ...props.ruleParameter,
              operator: foundValue.value,
            });
          }}
          allowDeselect={false}
          elevation={2}
        />
        {getValueInput()}
        {props.allowDelete && (
          <Group style={{ alignSelf: "stretch" }}>
            <ActionIcon
              color="var(--button-color-destructive)"
              size="sm"
              h="100%"
              onClick={() => props.doDelete?.(props.index)}
            >
              <Trash2Icon size={16} />
            </ActionIcon>
          </Group>
        )}
      </Group>
    </Card>
  );
};

export default ConditionItem;
