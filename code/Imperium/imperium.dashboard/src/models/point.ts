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

export interface Point {
  id: string;
  key: string;
  friendlyName: string;
  pointType: PointType;
  value?: string;
  lastUpdated: Date;
  pointState: PointState | undefined;
  readonly: boolean;
}
