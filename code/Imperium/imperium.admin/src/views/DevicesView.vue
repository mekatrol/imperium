<template>
  <h1>Devices</h1>

  <ul class="device-table">
    <li class="device-table-row">
      <p>Key</p>
      <p>Controller Key</p>
      <p>Enabled</p>
      <p>Online</p>
    </li>
    <li
      v-for="device in devices"
      :key="device.key"
      class="device-table-row"
    >
      <p>{{ device.key }}</p>
      <p>{{ device.controllerKey }}</p>
      <p>{{ device.enabled }}</p>
      <p>{{ device.online }}</p>
    </li>
  </ul>
</template>

<script setup lang="ts">
import { useIntervalTimer } from '@/composables/timer';
import type { Device } from '@/models/device';
import { useAppStore } from '@/stores/app-store';
import { ref } from 'vue';

const appStore = useAppStore();

const devices = ref<Device[]>([]);

useIntervalTimer(async () => {
  devices.value = await appStore.getDevices(() => {
    devices.value = [];
    return true;
  }, false);

  // Keep timer running
  return true;
}, 5000);
</script>

<style lang="css" scoped>
.device-table-row {
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
}

.device-table-row > p {
  width: 15%;
}
</style>
