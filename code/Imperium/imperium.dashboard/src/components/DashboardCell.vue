<template>
  <div :class="`dashboard-cell ${cssClass}`">
    <span v-if="icon" class="material-symbols-outlined">{{ icon }}</span>
    <p>{{ label }}</p>
  </div>
</template>

<script setup lang="ts">
interface Props {
  id: number;
  label: string;
  icon?: string;
  cssClass?: string;
}

defineProps<Props>();
</script>

<style lang="css" scoped>
.dashboard-cell {
  display: flex;
  flex-direction: row;
  align-content: center;
  justify-content: center;
  width: 100%;
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
