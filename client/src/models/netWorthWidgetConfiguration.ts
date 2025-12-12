export interface INetWorthWidgetLineCreateRequest {
  name: string;
  group: number;
  index: number;
  widgetSettingsId: string;
}

export interface INetWorthWidgetLineUpdateRequest {
  lineId: string;
  name: string;
  group: number;
  index: number;
  widgetSettingsId: string;
}

export interface INetWorthWidgetCategoryCreateRequest {
  value: string;
  type: string;
  subtype: string;
  lineId: string;
  widgetSettingsId: string;
}

export interface INetWorthWidgetCategoryUpdateRequest {
  Id: string;
  value: string;
  type: string;
  subtype: string;
  lineId: string;
  widgetSettingsId: string;
}
