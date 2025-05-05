<template>
  <main class="home">
    <div class="top-row">
      <div class="spacer spacer-left">
        <span
          v-if="serverStatusIcon"
          :class="`material-symbols-outlined ${serverStatusClass}`"
          >{{ serverStatusIcon }}</span
        >
      </div>
      <div class="time-card">
        <p class="time">{{ timeDisplay }}</p>
        <div style="display: flex; flex-direction: row; gap: 30px">
          <div
            class="sunrise"
            v-if="sunrisePoint"
          >
            <span class="material-symbols-outlined">sunny</span> {{ getTimeWithMeridiem(new Date(sunrisePoint.value! as Date), false) }}
          </div>
          <div>
            <p class="date"><span class="material-symbols-outlined">calendar_month</span>{{ dateDisplay }}</p>
          </div>
          <div
            class="sunset"
            v-if="sunsetPoint"
          >
            <span class="material-symbols-outlined">routine</span>{{ getTimeWithMeridiem(new Date(sunsetPoint.value! as Date), false) }}
          </div>
        </div>
      </div>
      <div class="spacer spacer-right">
        <span :class="`material-symbols-outlined ${isDaytimeClass}`">{{ isDaytimePoint?.value ? 'wb_sunny' : 'dark_mode' }}</span>
      </div>
    </div>
    <div class="dashboard">
      <div
        class="cell-container"
        v-for="cell in gridCells"
        :key="cell.props.id"
        :class="cell.props.cssClass"
      >
        <component
          :is="cell.component"
          v-bind="{ ...cell.props }"
        />
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useIntervalTimer } from '@/composables/timer';
import { getShortDateWithDay, getTimeWithMeridiem } from '@/services/date-helper';
import { computed, ref, shallowRef, type Component } from 'vue';
import { useAppStore } from '@/stores/app-store';
import DashboardSwitchCell from '@/components/DashboardSwitchCell.vue';
import { usePointStore } from '@/stores/point-store';

interface GridCellProps {
  id: number;
  icon?: string;
  cssClass?: string;
  valueDeviceKey: string;
  valuePointKey: string;
  countDownDeviceKey?: string;
  countDownPointKey?: string;
}

interface GridCell {
  component: Component;
  props: GridCellProps;
}

const appStore = useAppStore();
const pointStore = usePointStore();

const timeDisplay = ref('');
const dateDisplay = ref('');
const gridCells = shallowRef<GridCell[]>([]);

const isDaytimePoint = pointStore.initialisePointPoint('device.sunrisesunset', 'IsDaytime');
const sunsetPoint = pointStore.initialisePointPoint('device.sunrisesunset', 'Sunset');
const sunrisePoint = pointStore.initialisePointPoint('device.sunrisesunset', 'Sunrise');

const serverStatusIcon = computed((): string => {
  return appStore.serverOnline ? 'devices' : 'devices_off';
});

const serverStatusClass = computed((): string => {
  return appStore.serverOnline ? 'online' : 'offline';
});

const isDaytimeClass = computed((): string => {
  if (!isDaytimePoint.value) {
    return 'daytime-hide';
  }

  return isDaytimePoint.value.value ? 'daytime-day' : 'daytime-night';
});

const createCells = (): void => {
  let id = 0;
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'garage', valueDeviceKey: 'device.carport.powerboard', valuePointKey: 'Relay1' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'light', valueDeviceKey: 'device.frontdoorlight', valuePointKey: 'Relay' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'looks_6', valueDeviceKey: 'device.housenumberlight', valuePointKey: 'Relay' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'checkroom', valueDeviceKey: 'device.clothesline', valuePointKey: 'Relay' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'heat_pump_balance', valueDeviceKey: 'virtual', valuePointKey: 'water.pumps' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'light', valueDeviceKey: 'device.alfrescolight', valuePointKey: 'Relay' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'light', valueDeviceKey: 'device.kitchen.light', valuePointKey: 'Relay' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'light', valueDeviceKey: 'device.kitchenview.powerboard', valuePointKey: 'Relay1' } });

  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'handyman', valueDeviceKey: '', valuePointKey: '', cssClass: 'grid-two-row' } });
  gridCells.value.push({ component: DashboardSwitchCell, props: { id: id++, icon: 'e911_emergency', valueDeviceKey: 'virtual', valuePointKey: 'panic', cssClass: 'grid-two-col grid-two-row panic' } });
  // gridCells.value.push({ component: DashboardTemperatureControllerCell, props: { id: id++, icon: 'pets', valueDeviceKey: '', valuePointKey: '', cssClass: 'grid-two-row' } });
};

createCells();

const updateDateTime = (): void => {
  const dt = new Date();
  timeDisplay.value = getTimeWithMeridiem(dt);
  dateDisplay.value = getShortDateWithDay(dt);
};

useIntervalTimer(async () => {
  // Update the date and time
  updateDateTime();

  // Keep timer running
  return true;
}, 1000);

// const getApplicationExecutionVersion = async (): Promise<string> => {
//   const appVersion = await appStore.getApplicationVersion(() => {
//     return true;
//   }, false);

//   return appVersion.executionVersion;
// };

// useIntervalTimer(async () => {
//   // Get any updated application exectuion version
//   const serverApplicationExecutionVersion = await getApplicationExecutionVersion();

//   const params = new URLSearchParams(window.location.search);
//   const applicationExecutionVersion = params.get('v');

//   if (serverApplicationExecutionVersion != applicationExecutionVersion) {
//     // Reload the page using the new version
//     const updatedUrl = location.protocol + '//' + location.host + location.pathname + `?v=${serverApplicationExecutionVersion}`;
//     window.location.replace(updatedUrl);
//   }

//   // Keep timer running
//   return true;
// }, 5000);

updateDateTime();
</script>

<style lang="css">
.home {
  min-width: 800px;
  max-width: 800px;
  min-height: 480px;
  max-height: 480px;
  background-color: var(--clr-dashboard-background);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.top-row {
  display: flex;
  flex-direction: row;
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
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+2074, U+20AC, U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
  }

  .time {
    font-size: 5rem;
    color: var(--clr-time);
  }

  .date,
  .sunrise,
  .sunset {
    height: 100%;
    display: flex;
    flex-direction: row;
    gap: 5px;
    align-content: center;
    justify-content: center;
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

.dashboard > * {
  display: flex;
  line-height: 4rem;
  border-radius: 5px;
  align-content: center;
  justify-content: center;
}

.spacer {
  min-width: 80px;
}

.spacer-left,
.spacer-right {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-content: center;
}

.spacer-left span,
.spacer-right span {
  text-align: center;
  font-size: 4rem;
}

.spacer-left span.offline {
  text-align: center;
  font-size: 4rem;
  color: var(--clr-state-offline);
}

.spacer-left span.online {
  text-align: center;
  font-size: 4rem;
  color: var(--clr-state-on);
}

.daytime-hide {
  color: transparent;
}

.daytime-day {
  color: #ffff00;
}

.daytime-night {
  color: #aaa;
}
</style>
