import {
  ActionIcon,
  Card,
  Group,
  NumberInput,
  Select,
  TextInput,
} from "@mantine/core";
import { DateInput } from "@mantine/dates";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { Trash2Icon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import CategorySelect from "~/components/CategorySelect";
import { getCurrencySymbol } from "~/helpers/currency";
import {
  FieldToOperatorType,
  IRuleParameterCreateRequest,
  Operators,
  TransactionFields,
} from "~/models/automaticCategorizationRule";
import { ICategory } from "~/models/category";
import { IUserSettings } from "~/models/userSettings";

export interface ConditionItemProps {
  ruleParameter: IRuleParameterCreateRequest;
  setRuleParameter: (newParameter: IRuleParameterCreateRequest) => void;
  allowDelete?: boolean;
  doDelete?: (index: number) => void;
  index: number;
  categories: ICategory[];
}

const ConditionItem = (props: ConditionItemProps): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

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
          value={props.ruleParameter.value}
          onChange={(event) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value: event.currentTarget.value,
            })
          }
        />
      );
    } else if (props.ruleParameter.field === "amount") {
      return (
        <NumberInput
          flex="1 1 auto"
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
        />
      );
    } else if (props.ruleParameter.field === "date") {
      return (
        <DateInput
          flex="1 1 auto"
          value={props.ruleParameter.value}
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value: value ?? "",
            })
          }
        />
      );
    } else if (props.ruleParameter.field === "category") {
      return (
        <CategorySelect
          value={props.ruleParameter.value}
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              value,
            })
          }
          categories={props.categories}
          withinPortal
          flex="1 1 auto"
        />
      );
    }

    return null;
  };

  return (
    <Card p="0.5rem" radius="md">
      <Group gap="0.5rem">
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
              operator:
                Operators.filter(
                  (op) =>
                    op.type ===
                    FieldToOperatorType.get(
                      TransactionFields.find((field) => field.label === value)
                        ?.value ?? ""
                    )
                ).at(0)?.value ?? "",
              value: "",
              type:
                FieldToOperatorType.get(
                  TransactionFields.find((field) => field.label === value)
                    ?.value ?? ""
                ) ?? "",
            })
          }
          allowDeselect={false}
        />
        <Select
          w="130px"
          data={Operators.filter(
            (op) =>
              op.type === FieldToOperatorType.get(props.ruleParameter.field)
          ).map((op) => op.label)}
          value={
            Operators.find((op) => op.value === props.ruleParameter.operator)
              ?.label ?? ""
          }
          onChange={(value) =>
            props.setRuleParameter({
              ...props.ruleParameter,
              operator: Operators.find((op) => op.label === value)?.value ?? "",
            })
          }
          allowDeselect={false}
        />
        {getValueInput()}
        {props.allowDelete && (
          <Group style={{ alignSelf: "stretch" }}>
            <ActionIcon
              color="red"
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
