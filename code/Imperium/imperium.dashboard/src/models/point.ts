export enum PointType {
  Integer = 'integer',
  SingleFloat = 'singleFloat',
  DoubleFloat = 'doubleFloat',
  Boolean = 'boolean',
  String = 'string',
  DateTime = 'dateTime',
  DateOnly = 'dateOnly',
  TimeOnly = 'timeOnly',
  TimeSpan = 'timeSpan'
}

export enum PointState {
  Offline = 'offline',
  Online = 'online'
}

export type PointTypes = object | string | number | boolean | Date | undefined;

export interface Point {
  id: string;
  key: string;
  deviceKey: string;
  friendlyName: string;
  pointType: PointType;
  value?: PointTypes;
  lastUpdated: Date;
  pointState: PointState | undefined;
  readonly: boolean;
}

export interface CountdownPoint {
  valuePoint: Point;
  countdownPoint: Point;
}

export interface PointValueUpdate {
  id: string;
  value: PointTypes;
}
