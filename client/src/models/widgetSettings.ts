export interface IWidgetSettingsResponse {
  id: string;
  widgetType: string;
  lgX: number;
  lgY: number;
  lgW: number;
  lgH: number;
  smY: number;
  smH: number;
  configuration: string;
  userID: string;
}

export interface IWidgetSettingsCreateRequest {
  widgetType: string;
  lgX: number | null;
  lgY: number | null;
  lgW: number | null;
  lgH: number | null;
  smY: number | null;
  smH: number | null;
}

export interface IWidgetSettingsBatchUpdateRequest {
  id: string;
  lgX?: number | null;
  lgY?: number | null;
  lgW?: number | null;
  lgH?: number | null;
  smY?: number | null;
  smH?: number | null;
}

export interface IWidgetSettingsUpdateRequest<T> {
  id: string;
  lgX: number;
  lgY: number;
  lgW: number;
  lgH: number;
  smY: number;
  smH: number;
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
