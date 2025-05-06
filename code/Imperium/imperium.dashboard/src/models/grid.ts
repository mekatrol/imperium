import StatusIconCard from '@/components/StatusIconCard.vue';
import TimeCard from '@/components/TimeCard.vue';
import DashboardSwitchCell from '@/components/DashboardSwitchCell.vue';
import type { Component } from 'vue';

const componentMap = {
  StatusIconCard,
  TimeCard,
  DashboardSwitchCell
} as const;

type ComponentName = keyof typeof componentMap;

export const resolveComponent = (name: ComponentName): Component => {
  return componentMap[name] ?? null;
};

export interface GridItem {
  component: ComponentName;
  col: number;
  colSpan: number;
  row: number;
  rowSpan: number;
  cssClass?: string;
  props?: object;
}
