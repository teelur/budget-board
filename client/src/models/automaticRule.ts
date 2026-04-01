import { ComboboxItem } from "@mantine/core";

export interface IRuleParameterEdit {
  id?: string;
  field: string;
  operator: string;
  value: string;
}

export interface IRuleParameterCreateRequest {
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticRuleRequest {
  conditions: IRuleParameterCreateRequest[];
  actions: IRuleParameterCreateRequest[];
}

export interface IRuleParameterResponse {
  id: string;
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticRuleResponse {
  id: string;
  conditions: IRuleParameterResponse[];
  actions: IRuleParameterResponse[];
}

export interface IRuleParameterUpdateRequest {
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticRuleUpdateRequest {
  id: string;
  conditions: IRuleParameterUpdateRequest[];
  actions: IRuleParameterUpdateRequest[];
}

export interface TransactionField {
  label: string;
  value: string;
}

export const TransactionFields: ComboboxItem[] = [
  { label: "merchant_name", value: "merchant" },
  { label: "date", value: "date" },
  { label: "amount", value: "amount" },
  { label: "category", value: "category" },
];

export enum OperatorTypes {
  STRING = "string",
  NUMBER = "number",
  DATE = "date",
  CATEGORY = "category",
}

export const FieldToOperatorType = new Map<string, OperatorTypes>([
  ["merchant", OperatorTypes.STRING],
  ["date", OperatorTypes.DATE],
  ["amount", OperatorTypes.NUMBER],
  ["category", OperatorTypes.CATEGORY],
]);

export const ActionOperators: Operator[] = [
  { label: "set", value: "set", type: [] },
  { label: "delete_transaction", value: "delete", type: [] },
];

export interface Operator {
  label: string;
  value: string;
  type: OperatorTypes[];
}

export const Operators: Operator[] = [
  {
    label: "equals",
    value: "equals",
    type: [OperatorTypes.STRING, OperatorTypes.NUMBER],
  },
  {
    label: "does_not_equal",
    value: "notEquals",
    type: [OperatorTypes.STRING, OperatorTypes.NUMBER],
  },
  { label: "contains", value: "contains", type: [OperatorTypes.STRING] },
  {
    label: "does_not_contain",
    value: "doesNotContain",
    type: [OperatorTypes.STRING],
  },
  { label: "starts_with", value: "startsWith", type: [OperatorTypes.STRING] },
  { label: "ends_with", value: "endsWith", type: [OperatorTypes.STRING] },
  {
    label: "matches_regex",
    value: "matchesRegex",
    type: [OperatorTypes.STRING],
  },
  { label: "greater_than", value: "greaterThan", type: [OperatorTypes.NUMBER] },
  { label: "less_than", value: "lessThan", type: [OperatorTypes.NUMBER] },
  { label: "on", value: "on", type: [OperatorTypes.DATE] },
  { label: "before", value: "before", type: [OperatorTypes.DATE] },
  { label: "after", value: "after", type: [OperatorTypes.DATE] },
  { label: "is", value: "is", type: [OperatorTypes.CATEGORY] },
  { label: "is_not", value: "isNot", type: [OperatorTypes.CATEGORY] },
];
