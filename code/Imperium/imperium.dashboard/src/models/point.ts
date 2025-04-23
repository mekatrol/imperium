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

export enum PointUpdateAction {
  Control = 'control',
  Override = 'override',
  OverrideRelease = 'overrideRelease',
  Toggle = 'toggle'
}

export type PointTypes = object | string | number | boolean | Date | undefined | null;

export interface Point {
  id: string;
  key: string;
  deviceKey: string;
  friendlyName: string;
  pointType: PointType;
  value?: PointTypes;
  controlValue?: PointTypes;
  deviceValue?: PointTypes;
  overrideValue?: PointTypes;
  lastUpdated: Date;
  pointState: PointState | undefined;
  readonly: boolean;
}

export interface CountdownPoint {
  valuePoint: Point;
  countdownPoint: Point | undefined;
}

export interface PointValueUpdate {
  deviceKey: string;
  pointKey: string;
  pointUpdateAction: PointUpdateAction;
  value: PointTypes;
}
