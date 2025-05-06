export interface GridItem {
  component: string;
  col: number;
  colSpan: number;
  row: number;
  rowSpan: number;
  cssClass?: string;
  props?: object;
}
