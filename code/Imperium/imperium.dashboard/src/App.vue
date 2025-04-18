<template>
  <BusyOverlay :show="isBusy" full-screen />
  <MessageOverlay :show="!!messageData" :data="messageData" full-screen />
  <RouterView />
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia';
import { RouterView } from 'vue-router';
import { useAppStore } from '@/stores/app-store';
import BusyOverlay from '@/components/BusyOverlay.vue';
import MessageOverlay from '@/components/MessageOverlay.vue';

const { messageData, isBusy } = storeToRefs(useAppStore());

</script>
