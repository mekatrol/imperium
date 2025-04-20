<template>
  <main class="home">
    <div class="top-row">
      <div class="spacer spacer-left">
        <span v-if="serverStatusIcon" :class="`material-symbols-outlined ${serverStatusClass}`">{{ serverStatusIcon
        }}</span>
      </div>
      <div class="time-card">
        <p class="time">{{ timeDisplay }}</p>
        <div style="display: flex; flex-direction: row; gap: 30px">
          <div class="sunrise" v-if="sunrisePoint"><span class="material-symbols-outlined">sunny</span> {{
            getTimeWithMeridiem(new Date(sunrisePoint.value! as Date), false) }}
          </div>
          <div>
            <p class="date"><span class="material-symbols-outlined">calendar_month</span>{{ dateDisplay }}</p>
          </div>
          <div class="sunset" v-if="sunsetPoint"><span class="material-symbols-outlined">routine</span>{{
            getTimeWithMeridiem(new Date(sunsetPoint.value! as Date), false) }}</div>
        </div>
      </div>
      <div class="spacer spacer-right"></div>
    </div>
    <div class="dashboard">
      <div class="cell-container" v-for="cell in gridCells" :key="cell.props.id" :class="cell.props.cssClass">
        <component :is="cell.component" v-bind="{ ...cell.props }" v-model="cell.model" />
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import { useIntervalTimer } from '@/composables/timer';
import { getShortDateWithDay, getTimeWithMeridiem } from '@/services/date-helper';
import { computed, ref, shallowRef, type Component, type Ref } from 'vue';
import { useAppStore } from '@/stores/app-store';
import type { CountdownPoint, Point } from '@/models/point';
import DashboardCell from '@/components/DashboardCell.vue';
import CountdownSwitch from '@/components/CountdownSwitch.vue';

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

const timeDisplay = ref('');
const dateDisplay = ref('');
const gridCells = shallowRef<GridCell[]>([]);
const allPoints = ref<Point[]>([]);
const sunsetPoint = ref<Point | undefined>();
const sunrisePoint = ref<Point | undefined>();
const clotheslinePoint = ref<Point | undefined>();
const alfrescoLightPoint = ref<Point | undefined>();
const kitchenCabinetLightsPoint = ref<CountdownPoint | undefined>();
const whiteStringLightsPoint = ref<Point | undefined>();
const aquaponicsPumpsPoint = ref<Point | undefined>();

const serverStatusIcon = computed((): string => {
  return appStore.serverOnline ? 'devices' : 'devices_off';
});

const serverStatusClass = computed((): string => {
  return appStore.serverOnline ? 'online' : 'offline';
});

const createCells = (): void => {
  let id = 0;
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Carport', icon: 'garage', state: 'on' } });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Front Door', icon: 'light', state: 'off' } });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'House Number', icon: 'looks_6' } });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Clothes Line', icon: 'checkroom' }, model: clotheslinePoint });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Water Pumps', icon: 'heat_pump_balance' }, model: aquaponicsPumpsPoint });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Alfresco', icon: 'light' }, model: alfrescoLightPoint });
  gridCells.value.push({ component: CountdownSwitch, props: { id: id++, label: 'Kitchen Cabinet', icon: 'light' }, model: kitchenCabinetLightsPoint });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'White String', icon: 'light' }, model: whiteStringLightsPoint });

  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'Garage', icon: 'handyman', cssClass: 'grid-two-row' } });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'PANIC', icon: 'e911_emergency', cssClass: 'grid-two-col grid-two-row' } });
  gridCells.value.push({ component: DashboardCell, props: { id: id++, label: 'More', icon: 'arrow_right_alt', cssClass: 'grid-two-row' } });
};

createCells();

const updateDateTime = (): void => {
  const dt = new Date();
  timeDisplay.value = getTimeWithMeridiem(dt);
  dateDisplay.value = getShortDateWithDay(dt);
};

const updatePoint = (deviceKey: string | null, pointKey: string, point: Ref<Point | undefined>): void => {
  const points = allPoints.value.filter(p => p.deviceKey === deviceKey && p.key === pointKey);
  if (points.length === 1) {
    point.value = points[0];
  } else {
    point.value = undefined;
  }
};

const updateCountdown = (
  valueDeviceKey: string, valuePointKey: string,
  countdownDeviceKey: string | null, countdownPointKey: string,
  point: Ref<CountdownPoint | undefined>): void => {

  const valuePoints = allPoints.value.filter(p => p.deviceKey === valueDeviceKey && p.key === valuePointKey);
  const countdownPoints = allPoints.value.filter(p => p.deviceKey == countdownDeviceKey && p.key === countdownPointKey);

  let valuePoint: Point | undefined = undefined;
  let countdownPoint: Point | undefined = undefined;

  if (valuePoints.length === 1) {
    valuePoint = valuePoints[0];
  } else {
    point.value = undefined;
  }

  if (countdownPoints.length === 1) {
    countdownPoint = countdownPoints[0];
  } else {
    point.value = undefined;
  }

  if (!valuePoint || !countdownPoint) {
    point.value = undefined;
    return;
  }

  point.value = {
    valuePoint: valuePoint,
    countdownPoint: countdownPoint
  };
};

const updatePoints = (): void => {
  updatePoint('device.clothesline', 'Relay', clotheslinePoint);
  updatePoint('device.alfrescolight', 'Relay', alfrescoLightPoint);
  updateCountdown('device.kitchen.light', 'Relay', null, 'kitchen.light.timer', kitchenCabinetLightsPoint);
  updatePoint('device.kitchenview.powerboard', 'Relay1', whiteStringLightsPoint);
  updatePoint(null, 'water.pumps', aquaponicsPumpsPoint);
};

useIntervalTimer(async () => {
  // Update the date and time
  updateDateTime();

  try {
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

    updatePoints();

    appStore.setServerOnlineStatus(true);
  }
  catch {
    allPoints.value = [];
    updatePoints();

    appStore.setServerOnlineStatus(false);
  }

  // Keep timer running
  return true;
}, 1000);

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
    unicode-range: U+0000-00FF, U+0131, U+0152-0153, U+02BB-02BC, U+02C6, U+02DA, U+02DC, U+0304, U+0308, U+0329, U+2000-206F, U+2074, U+20AC,
      U+2122, U+2191, U+2193, U+2212, U+2215, U+FEFF, U+FFFD;
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

.dashboard>* {
  display: flex;
  line-height: 4rem;
  border-radius: 5px;
  align-content: center;
  justify-content: center;
}

.spacer {
  min-width: 80px;
}

.spacer-left {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-content: center;
}

.spacer-left span {
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
</style>
