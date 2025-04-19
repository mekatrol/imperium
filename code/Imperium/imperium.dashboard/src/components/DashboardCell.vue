<template>
  <div :class="`dashboard-cell${cssClass ? ' ' + cssClass : ''} ${getCssClass()} `">
    <span v-if="icon" class="material-symbols-outlined">{{ icon }}</span>
    <p>{{ label }}</p>
  </div>
</template>

<script setup lang="ts">
import type { Point } from '@/models/point';
import type { Ref } from 'vue';

interface Props {
  id: number;
  label: string;
  icon?: string;
  cssClass?: string;
  state?: string | undefined;
}

const model = defineModel<Ref<Point>>();
defineProps<Props>();

const getCssClass = (): string => {
  if (model.value?.value === undefined) {
    return 'state-offline';
  }

  return (model.value.value.value === 1 || model.value.value.value === true) ? 'state-on' : 'state-off';
};
</script>

<style lang="css">
:root {
  --clr-state-offline: #991503;
  --clr-state-off: #838282;
  --clr-state-on: #01a301;
}

.dashboard-cell {
  display: flex;
  flex-direction: row;
  align-content: center;
  justify-content: center;
  width: 100%;
  outline-offset: -1px;
}

.dashboard-cell.state-offline {
  color: var(--clr-state-offline);
  outline: 1px solid var(--clr-state-offline);
}

.dashboard-cell.state-off {
  color: var(--clr-state-off);
  outline: 1px solid var(--clr-state-off);
}

.dashboard-cell.state-on {
  color: var(--clr-state-on);
  outline: 1px solid var(--clr-state-on);
}

.dashboard-cell>span {
  margin: auto;
  font-size: 3rem;
  align-self: flex-start;
}

.dashboard-cell>p {
  margin: auto;
  font-size: 1rem;
}

.two_row .grid-cell>span {
  font-size: 5rem;
}

.two_column {
  grid-column-start: 2;
  grid-column-end: 4;
}
</style>
