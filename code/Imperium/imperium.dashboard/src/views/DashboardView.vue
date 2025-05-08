<template>
  <DashboardGrid
    v-if="dashboard"
    :gap="0"
    :columns="16"
    :rows="16"
    :items="dashboard.items"
  />
</template>

<script setup lang="ts">
import { useAppStore } from '@/stores/app-store';

import DashboardGrid from '@/components/DashboardGrid.vue';
import { storeToRefs } from 'pinia';

const appStore = useAppStore();

const { dashboard } = storeToRefs(appStore);
</script>

<style lang="css">
.padded-cell {
  padding: 0.5rem;
  padding-top: 0.8rem;
}
</style>
