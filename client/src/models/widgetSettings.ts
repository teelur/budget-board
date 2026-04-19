export interface IWidgetSettingsResponse {
  id: string;
  widgetType: string;
  x: number;
  y: number;
  w: number;
  h: number;
  configuration: string;
  userID: string;
}

export interface IWidgetSettingsCreateRequest {
  widgetType: string;
  x: number;
  y: number;
  w: number;
  h: number;
}

export interface IWidgetSettingsBatchUpdateRequest {
  id: string;
  x: number;
  y: number;
  w: number;
  h: number;
}

export interface IWidgetSettingsUpdateRequest<T> {
  id: string;
  x: number;
  y: number;
  w: number;
  h: number;
  configuration: T;
}

export interface IAccountsWidgetConfiguration {
  accountIds: string[];
}

export interface INetWorthWidgetConfiguration {
  groups: Array<INetWorthWidgetGroup>;
}

export interface INetWorthWidgetGroup {
  id: string;
  index: number;
  lines: Array<INetWorthWidgetLine>;
}

export interface INetWorthWidgetLine {
  id: string;
  name: string;
  categories: Array<INetWorthWidgetCategory>;
  index: number;
}

export interface INetWorthWidgetCategory {
  id: string;
  value: string;
  type: string;
  subtype: string;
}
