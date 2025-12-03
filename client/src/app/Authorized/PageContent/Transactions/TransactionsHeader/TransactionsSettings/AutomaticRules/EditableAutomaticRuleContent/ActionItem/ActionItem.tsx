import { ActionIcon, Group } from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { Trash2Icon } from "lucide-react";
import React from "react";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { getDefaultValue } from "~/helpers/automaticRules";
import { getCurrencySymbol } from "~/helpers/currency";
import {
  ActionOperators,
  IRuleParameterEdit,
  TransactionFields,
} from "~/models/automaticRule";
import { ICategory } from "~/models/category";
import { IUserSettings } from "~/models/userSettings";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";
import Card from "~/components/core/Card/Card";

export interface ActionItemProps {
  ruleParameter: IRuleParameterEdit;
  setRuleParameter: (newParameter: IRuleParameterEdit) => void;
  allowDelete: boolean;
  doDelete: (index: number) => void;
  index: number;
  categories: ICategory[];
}

const ActionItem = (props: ActionItemProps): React.ReactNode => {
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
          placeholder="Enter merchant"
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
          placeholder="Enter amount"
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
          placeholder="Select date"
          value={props.ruleParameter.value}
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

  const getCardContent = (): React.ReactNode => {
    if (props.ruleParameter.operator === "delete") {
      return <PrimaryText size="sm">the transaction</PrimaryText>;
    } else if (props.ruleParameter.operator === "set") {
      return (
        <>
          <Select
            w="110px"
            data={TransactionFields.map((field) => field.label)}
            value={
              TransactionFields.find(
                (field) => field.value === props.ruleParameter.field
              )?.label ?? ""
            }
            onChange={(value) =>
              props.setRuleParameter({
                ...props.ruleParameter,
                field:
                  TransactionFields.find((field) => field.label === value)
                    ?.value ?? "",
                value: getDefaultValue(
                  TransactionFields.find((field) => field.label === value)
                    ?.value ?? ""
                ),
              })
            }
            elevation={2}
          />
          <PrimaryText size="sm">to</PrimaryText>
          {getValueInput()}
        </>
      );
    }
    return null;
  };

  return (
    <Card elevation={2}>
      <Group gap="0.5rem">
        <Select
          w="90px"
          data={ActionOperators.map((op) => op.label)}
          value={
            ActionOperators.find(
              (op) => op.value === props.ruleParameter.operator
            )?.label ?? ""
          }
          onChange={(value) => {
            props.setRuleParameter({
              ...props.ruleParameter,
              operator:
                ActionOperators.find((op) => op.label === value)?.value ??
                ActionOperators[0]!.value,
            });
          }}
          elevation={2}
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
