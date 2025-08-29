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
