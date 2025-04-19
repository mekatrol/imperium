<template>
  <main class="home">
    <div class="time-card">
      <p class="time">{{ timeDisplay }}</p>
      <div style="display: flex; flex-direction: row; gap: 30px">
        <p class="sunrise" v-if="sunrisePoint">Sunrise: {{ getTimeWithMeridiem(new Date(sunrisePoint.value!), false) }}
        </p>
        <p class="date">{{ dateDisplay }}</p>
        <p class="sunset" v-if="sunsetPoint">Sunset: {{ getTimeWithMeridiem(new Date(sunsetPoint.value!), false) }}</p>
      </div>
    </div>
    <div class="dashboard">
      <component v-for="cell in gridCells" :key="cell.props.id" :is="cell.component" v-bind="{ ...cell.props }"
        v-model="cell.model" />
    </div>
  </main>
</template>

<script setup lang="ts">
import { useIntervalTimer } from '@/composables/timer';
import { getShortDateWithDay, getTimeWithMeridiem } from '@/services/date-helper';
import { ref, shallowRef, type Component } from 'vue';

import DashboardCell from '@/components/DashboardCell.vue';
import { useAppStore } from '@/stores/app-store';
import type { Point } from '@/models/point';

interface GridCellProps {
  id: number;
  label: string;
  icon?: string;
  cssClass?: string;
  state?: string | undefined;
}

interface GridCell {
  component: Component;
  props: GridCellProps;
  model?: unknown;
}

const appStore = useAppStore();

const gridCells = shallowRef<GridCell[]>([]);
const allPoints = ref<Point[]>([]);
const sunsetPoint = ref<Point | undefined>();
const sunrisePoint = ref<Point | undefined>();

let id = 0;
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Carport', icon: 'garage', state: 'on' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Front Door', icon: 'light', state: 'off' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'House Number', icon: 'looks_6' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Clothes Line', icon: 'checkroom' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'BBQ Colour', icon: 'light' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Alfresco', icon: 'light' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Kitchen Cabinet', icon: 'light' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'White String', icon: 'light' } });

gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Garage', icon: 'handyman', cssClass: 'two_row' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'PANIC', icon: 'e911_emergency', cssClass: 'two_column two_row' } });
gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'More', icon: 'arrow_right_alt', cssClass: 'two_row' } });

const timeDisplay = ref('');
const dateDisplay = ref('');

const updateDateTime = (): void => {
  const dt = new Date();
  timeDisplay.value = getTimeWithMeridiem(dt);
  dateDisplay.value = getShortDateWithDay(dt);
};

useIntervalTimer(async () => {
  // Update the date and time
  updateDateTime();

  // Update points
  const points = await appStore.getPoints(() => { return true; }, false);
  allPoints.value = points;

  const sunrise = points.filter(p => p.key === 'Sunrise');
  if (sunrise.length === 1) {
    sunrisePoint.value = sunrise[0];
  }

  const sunset = points.filter(p => p.key === 'Sunset');
  if (sunset.length === 1) {
    sunsetPoint.value = sunset[0];
  }

  // Keep timer running
  return true;
}, 1000);

updateDateTime();

</script>

<style lang="css">
:root {
  --clr-time: #ff0000;
  --clr-date: #55ff88;
  --clr-sunrise: #f1e20f;
  --clr-sunset: #f7621e;
  --clr-dashboard-background: #222;
  --clr-grid-cell-outline: #ccc;
}

.home {
  min-width: 800px;
  max-width: 800px;
  min-height: 480px;
  max-height: 480px;
  background-color: var(--clr-dashboard-background);
}

.time-card {
  display: flex;
  flex-direction: column;
  gap: 0;
  align-items: center;
  font-family: 'Orbitron';
  width: 100%;

  @font-face {
    font-family: 'Orbitron';
    font-style: normal;
    font-weight: 400 900;
    font-display: swap;
    src: url(https://fonts.gstatic.com/s/orbitron/v31/yMJRMIlzdpvBhQQL_Qq7dy0.woff2) format('woff2');
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+2074, U+20AC,
      U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
  }

  .time {
    font-size: 5rem;
    color: var(--clr-time);
  }

  .date {
    font-size: 1.3rem;
    color: var(--clr-date);
  }

  .sunrise {
    font-size: 1.3rem;
    color: var(--clr-sunrise);
  }

  .sunset {
    font-size: 1.3rem;
    color: var(--clr-sunset);
  }
}

.dashboard {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr 1fr;
  grid-template-rows: 80px 80px 140px;
  padding: 10px;
  gap: 10px;
  margin-top: 1rem;
}

.dashboard>* {
  display: flex;
  line-height: 4rem;
  border-radius: 5px;
  align-content: center;
  justify-content: center;
}
</style>
