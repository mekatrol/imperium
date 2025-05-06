import type { Component } from 'vue';

export interface GridItem {
  component: Component;
  col: number;
  colSpan: number;
  row: number;
  rowSpan: number;
  cssClass?: string;
  props?: object;
}
