export interface INetWorthWidgetLineUpdateRequest {
  lineId: string;
  name: string;
  group: number;
  index: number;
  widgetSettingsId: string;
}

export interface INetWorthWidgetLineCreateRequest {
  name: string;
  group: number;
  index: number;
  widgetSettingsId: string;
}
