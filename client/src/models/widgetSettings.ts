export interface IWidgetSettingsResponse {
  id: string;
  widgetType: string;
  isVisible: boolean;
  configuration: string;
  userID: string;
}

export interface IWidgetSettingsUpdateRequest<T> {
  id: string;
  isVisible: boolean;
  configuration: T;
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
