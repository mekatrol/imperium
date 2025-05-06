<template>
  <div
    class="grid"
    :style="gridStyle()"
  >
    <div
      v-for="(item, i) in items"
      :key="i"
      :class="`item ${item.cssClass ?? ''}`"
      :style="`${getSpan(item.col, item.colSpan, item.row, item.rowSpan)}`"
    >
      <component
        :is="item.component"
        v-bind="{ ...item.props }"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { GridItem } from '@/models/grid';

interface Props {
  gap?: number;
  width?: number;
  height?: number;
  columns?: number;
  rows?: number;
  items: GridItem[];
}

const props = withDefaults(defineProps<Props>(), {
  gap: 10,
  width: 800,
  height: 480,
  columns: 6,
  rows: 6,
  items: () => []
});

const gridStyle = (): string => {
  const columns = `grid-template-columns: repeat(${props.columns}, 1fr);`;
  const rows = `grid-template-rows: repeat(${props.rows}, ${props.height / props.rows}px);`;

  return `gap: ${props.gap}px; width: 100%; max-width: ${props.width}px; max-height: ${props.height}px; ${columns} ${rows}`;
};

const getSpan = (col: number, colSpan: number, row: number, rowSpan: number): string => {
  return `grid-column: ${col} / span ${colSpan}; grid-row: ${row} / span ${rowSpan};`;
};
</script>

<style lang="css" scoped>
.grid {
  display: grid;
}

.item {
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}
</style>
