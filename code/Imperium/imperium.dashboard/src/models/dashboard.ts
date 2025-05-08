import StatusIconCard from '@/components/StatusIconCard.vue';
import TimeCard from '@/components/TimeCard.vue';
import DashboardSwitchCell from '@/components/DashboardSwitchCell.vue';
import type { Component } from 'vue';

export interface StateUpdate {
  (): boolean;
}

const componentMap = {
  StatusIconCard,
  TimeCard,
  DashboardSwitchCell
} as const;

type ComponentName = keyof typeof componentMap;

export const resolveComponent = (name: ComponentName): Component => {
  return componentMap[name] ?? null;
};

interface DashboardItemProps {
  state?: StateUpdate;
  deviceKey?: string;
  pointKey?: string;
}

export interface DashboardItem {
  componentName: ComponentName;
  column: number;
  columnSpan: number;
  row: number;
  rowSpan: number;
  cssClass?: string;
  props?: DashboardItemProps;
}

export interface Dashboard {
  items: DashboardItem[];
}
