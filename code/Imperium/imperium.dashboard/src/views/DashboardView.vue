<template>
  {{ appStore.serverOnline }}
  <DashboardGrid
    :gap="0"
    :columns="16"
    :rows="16"
    :items="gridItems"
  />
</template>

<script setup lang="ts">
import type { GridItem } from '@/models/grid';
import { useAppStore } from '@/stores/app-store';
import { usePointStore } from '@/stores/point-store';

import DashboardGrid from '@/components/DashboardGrid.vue';
import { storeToRefs } from 'pinia';

const appStore = useAppStore();
const pointStore = usePointStore();

const { serverOnline } = storeToRefs(appStore);

const isDaytimePoint = pointStore.getPoint('device.sunrisesunset', 'IsDaytime');

const gridItems: GridItem[] = [
  {
    component: 'StatusIconCard',
    col: 1,
    colSpan: 2,
    row: 1,
    rowSpan: 4,
    props: {
      iconOff: 'devices_off',
      iconOn: 'devices',
      colorOff: '#991503',
      state: () => serverOnline.value
    }
  },
  {
    component: 'TimeCard',
    col: 3,
    colSpan: 12,
    row: 1,
    rowSpan: 4,
    cssClass: 'time-card-cell'
  },
  {
    component: 'StatusIconCard',
    col: 15,
    colSpan: 2,
    row: 1,
    rowSpan: 4,
    props: {
      iconOff: 'dark_mode',
      iconOn: 'wb_sunny',
      colorOn: '#ffff00',
      colorOff: '#aaa',
      state: () => isDaytimePoint.value?.value === true
    }
  },
  {
    component: 'DashboardSwitchCell',
    col: 1,
    colSpan: 4,
    row: 11,
    rowSpan: 4,
    cssClass: 'padded-cell',
    props: {
      icon: 'handyman',
      valueDeviceKey: 'virtual',
      valuePointKey: 'panic'
    }
  },
  {
    component: 'DashboardSwitchCell',
    col: 5,
    colSpan: 8,
    row: 11,
    rowSpan: 4,
    cssClass: 'padded-cell',
    props: {
      icon: 'e911_emergency',
      valueDeviceKey: 'virtual',
      valuePointKey: 'panic'
    }
  },
  {
    component: 'DashboardSwitchCell',
    col: 13,
    colSpan: 4,
    row: 11,
    rowSpan: 4,
    cssClass: 'padded-cell',
    props: {
      icon: 'pets',
      valueDeviceKey: 'virtual',
      valuePointKey: 'panic'
    }
  }
];

let cellCol = 1;
let cellRow = 5;

const addNextCell = (icon: string, deviceKey: string, pointKey: string): void => {
  gridItems.push({
    component: 'DashboardSwitchCell',
    col: cellCol,
    colSpan: 4,
    row: cellRow,
    rowSpan: 3,
    cssClass: 'padded-cell',
    props: {
      icon: icon,
      valueDeviceKey: deviceKey,
      valuePointKey: pointKey
    }
  });

  cellCol += 4;

  if (cellCol > 16) {
    cellCol = 1;
    cellRow += 3;
  }
};

addNextCell('garage', 'device.carport.powerboard', 'Relay1');
addNextCell('light', 'device.frontdoorlight', 'Relay');
addNextCell('looks_6', 'device.housenumberlight', 'Relay');
addNextCell('checkroom', 'device.clothesline', 'Relay');
addNextCell('heat_pump_balance', 'virtual', 'water.pumps');
addNextCell('light', 'device.alfrescolight', 'Relay');
addNextCell('light', 'device.kitchen.light', 'Relay');
addNextCell('light', 'device.kitchenview.powerboard', 'Relay1');
</script>

<style lang="css">
.padded-cell {
  padding: 0.5rem;
  padding-top: 0.8rem;
}
</style>
