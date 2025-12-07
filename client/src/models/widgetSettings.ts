export interface IWidgetSettingsResponse {
  id: string;
  widgetType: string;
  isVisible: boolean;
  configuration: string;
  userID: string;
}

export interface INetWorthWidgetConfiguration {
  lines: Array<INetWorthWidgetLine>;
}

export interface INetWorthWidgetLine {
  name: string;
  categories: Array<INetWorthWidgetCategory>;
  group: number;
  index: number;
}

export interface INetWorthWidgetCategory {
  value: string;
  type: string;
  subtype: string;
}
