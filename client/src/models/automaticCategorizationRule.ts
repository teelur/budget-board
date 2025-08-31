export interface IRuleParameterCreateRequest {
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticCategorizationRuleRequest {
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

export interface IAutomaticCategorizationRuleResponse {
  id: string;
  conditions: IRuleParameterResponse[];
  actions: IRuleParameterResponse[];
}

export interface IRuleParameterUpdateRequest {
  id: string;
  field: string;
  operator: string;
  value: string;
  type: string;
}

export interface IAutomaticCategorizationRuleUpdateRequest {
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
}

export const FieldToOperatorType = new Map<string, OperatorTypes>([
  ["merchant", OperatorTypes.STRING],
  ["date", OperatorTypes.DATE],
  ["amount", OperatorTypes.NUMBER],
  ["category", OperatorTypes.STRING],
]);

export interface Operator {
  label: string;
  value: string;
  type: OperatorTypes;
}

export const Operators: Operator[] = [
  { label: "Equals", value: "equals", type: OperatorTypes.STRING },
  { label: "Contains", value: "contains", type: OperatorTypes.STRING },
  { label: "Starts With", value: "startsWith", type: OperatorTypes.STRING },
  { label: "Ends With", value: "endsWith", type: OperatorTypes.STRING },
  { label: "Greater Than", value: "greaterThan", type: OperatorTypes.NUMBER },
  { label: "Less Than", value: "lessThan", type: OperatorTypes.NUMBER },
  { label: "Before", value: "before", type: OperatorTypes.DATE },
  { label: "After", value: "after", type: OperatorTypes.DATE },
];
