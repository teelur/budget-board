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

export const TransactionFields = [
  { label: "Merchant", value: "merchant" },
  { label: "Date", value: "date" },
  { label: "Amount", value: "amount" },
  { label: "Category", value: "category" },
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

export const ActionOperation: string = "set";

export interface Operator {
  label: string;
  value: string;
  type: OperatorTypes;
}

export const Operators: Operator[] = [
  { label: "equals", value: "equals", type: OperatorTypes.STRING },
  {
    label: "does not equal",
    value: "notEquals",
    type: OperatorTypes.STRING,
  },
  { label: "contains", value: "contains", type: OperatorTypes.STRING },
  {
    label: "does not contain",
    value: "doesNotContain",
    type: OperatorTypes.STRING,
  },
  { label: "starts with", value: "startsWith", type: OperatorTypes.STRING },
  { label: "ends with", value: "endsWith", type: OperatorTypes.STRING },
  { label: "greater than", value: "greaterThan", type: OperatorTypes.NUMBER },
  { label: "less than", value: "lessThan", type: OperatorTypes.NUMBER },
  { label: "on", value: "on", type: OperatorTypes.DATE },
  { label: "before", value: "before", type: OperatorTypes.DATE },
  { label: "after", value: "after", type: OperatorTypes.DATE },
  { label: "is", value: "is", type: OperatorTypes.CATEGORY },
  { label: "is not", value: "isNot", type: OperatorTypes.CATEGORY },
];
