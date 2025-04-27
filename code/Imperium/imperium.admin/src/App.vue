<template>
  <nav>
    <hamburger-menu :menu-items="menuItems" />
  </nav>
  <RouterView />
</template>

<script setup lang="ts">
import HamburgerMenu from '@/components/HamburgerMenu.vue';
import type { MenuItem } from './models/menu-item';

const menuItems: MenuItem[] = [
  {
    label: 'Home',
    route: '/',
    enabled: true
  },
  {
    label: 'MQTT',
    route: '/mqtt',
    enabled: true
  }
];

</script>

<style lang="css" scoped>
nav {
  padding: 0.5rem;
}
</style>
