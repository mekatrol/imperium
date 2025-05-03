<template>
  <nav>
    <hamburger-menu :menu-items="routes" />
  </nav>
  <RouterView />
</template>

<script setup lang="ts">
import { routes } from '@/router/index';
import HamburgerMenu from '@/components/HamburgerMenu.vue';

</script>

<style lang="css" scoped>
nav {
  padding: 0.5rem;
}
</style>
