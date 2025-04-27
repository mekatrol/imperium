<template>
  <div class="hamburger-menu" @click.self="closeMenu">
    <input type="checkbox" id="menu-toggle" class="menu-toggle" />
    <label for="menu-toggle" class="menu-icon" @click.stop="toggleMenu">
      <span :class="{ open: isOpen }"></span>
      <span :class="{ open: isOpen }"></span>
      <span :class="{ open: isOpen }"></span>
    </label>
    <nav class="menu" v-if="isOpen">
      <ul>
        <li v-for="(menuItem, i) in menuItems" :key="i">
          <router-link :to="menuItem.route" @click="closeMenu">{{ menuItem.label }}</router-link>
        </li>
      </ul>
    </nav>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { MenuItem } from '@/models/menu-item';

interface Props {
  menuItems: MenuItem[];
}

defineProps<Props>();

const isOpen = ref(false);

const toggleMenu = (): void => {
  isOpen.value = !isOpen.value;
};

const closeMenu = (): void => {
  isOpen.value = false;
};

</script>

<style scoped>
.hamburger-menu {
  position: relative;
}

/* Hide the checkbox */
.menu-toggle {
  display: none;
}

.menu-icon {
  width: 30px;
  height: 25px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  cursor: pointer;
  z-index: 2;
}

.menu-icon span {
  display: block;
  height: 4px;
  background: #333;
  border-radius: 2px;
  transition: 0.4s;
}

/* Animate lines into an "X" */
/* .menu-toggle:checked+.menu-icon span:nth-child(1) {
  transform: rotate(45deg) translate(5px, 10px);
}

.menu-toggle:checked+.menu-icon span:nth-child(2) {
  opacity: 0;
}

.menu-toggle:checked+.menu-icon span:nth-child(3) {
  transform: rotate(-45deg) translate(5px, -10px);
} */

/* Animate into an X */
.menu-icon span.open:nth-child(1) {
  transform: rotate(45deg) translate(5px, 5px);
}

.menu-icon span.open:nth-child(2) {
  opacity: 0;
}

.menu-icon span.open:nth-child(3) {
  transform: rotate(-45deg) translate(5px, -5px);
}

/* Menu styles */
.menu {
  position: absolute;
  top: 40px;
  left: 0;
  background: white;
  border: 1px solid #ddd;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  width: 200px;
  overflow: hidden;
  max-height: 0;
  opacity: 0;
  transition: max-height 0.4s ease, opacity 0.4s ease;
}

.menu ul {
  list-style: none;
  margin: 0;
  padding: 0;
}

.menu li {
  border-bottom: 1px solid #eee;
}

.menu li:last-child {
  border-bottom: none;
}

.menu a {
  display: block;
  padding: 10px 20px;
  color: #333;
  text-decoration: none;
}

.menu a:hover {
  background: #f5f5f5;
}

/* Show menu with animation */
.menu-toggle:checked+.menu-icon+.menu {
  max-height: 500px;
  /* large enough to fit all items */
  opacity: 1;
}
</style>
