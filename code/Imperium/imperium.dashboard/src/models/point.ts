export enum PointType {
  Integer = 'Integer',
  SingleFloat = 'SingleFloat',
  DoubleFloat = 'DoubleFloat',
  Boolean = 'Boolean',
  String = 'String',
  DateTime = 'DateTime',
  DateOnly = 'DateOnly',
  TimeOnly = 'TimeOnly',
  TimeSpan = 'TimeSpan'
}

export enum PointState {
  Offline = 'Offline',
  Online = 'Online'
}

export enum PointUpdateAction {
  Control = 'Control',
  Override = 'Override',
  OverrideRelease = 'OverrideRelease',
  Toggle = 'Toggle'
}

export type PointTypes = object | string | number | boolean | Date | undefined | null;

export interface Point {
  key: string;
  deviceKey: string;
  friendlyName?: string;
  pointType: PointType;
  value?: PointTypes;
  controlValue?: PointTypes;
  deviceValue?: PointTypes;
  overrideValue?: PointTypes;
  lastUpdated?: Date;
  pointState: PointState | undefined;
  readonly: boolean;
}

export interface TemperatureControlPoint {
  valuePoint: Point;
  setpointPoint: Point;
  proportionalBandPoint: Point;
  enabledPoint: Point;
  onPoint: Point;
}

export interface PointValueUpdate {
  deviceKey: string;
  pointKey: string;
  pointUpdateAction: PointUpdateAction;
  value: PointTypes;
}
