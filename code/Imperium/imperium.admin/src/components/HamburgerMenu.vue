<template>
  <div
    class="hamburger-menu"
    @click.self="closeMenu"
    ref="menuRef"
  >
    <input
      type="checkbox"
      id="menu-toggle"
      class="menu-toggle"
      v-model="checked"
    />
    <label
      for="menu-toggle"
      class="menu-icon"
      @click.stop="toggleMenu"
    >
      <span :class="{ open: isOpen }"></span>
      <span :class="{ open: isOpen }"></span>
      <span :class="{ open: isOpen }"></span>
    </label>
    <nav class="menu">
      <ul>
        <li
          v-for="(menuItem, i) in menuItems"
          :key="i"
        >
          <router-link
            :to="menuItem.path"
            @click="closeMenu"
            ><span>
              <span
                v-if="menuItem.icon"
                class="material-symbols-outlined"
                >{{ menuItem.icon }}</span
              >
            </span>
            <span>{{ menuItem.label }}</span></router-link
          >
        </li>
      </ul>
    </nav>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue';
import type { NavItem } from '@/models/app-models';

interface Props {
  menuItems: NavItem[];
}

defineProps<Props>();

const isOpen = ref(false);
const checked = ref();

const toggleMenu = (): void => {
  isOpen.value = !isOpen.value;
};

const closeMenu = (): void => {
  isOpen.value = false;
  checked.value = false;
};

// Close menu if clicked outside
const menuRef = ref<HTMLElement | null>(null);

const handleClickOutside = (event: MouseEvent): void => {
  if (menuRef.value && !menuRef.value.contains(event.target as Node)) {
    closeMenu();
  }
};

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', handleClickOutside);
});
</script>

<style scoped>
.hamburger-menu {
  position: relative;
}

/* The checkbox is hidden and is used for CSS selectors */
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
  background: #929292;
  border-radius: 2px;
  transition: 0.4s;
}

/* Animate into an X */
.menu-icon span.open:nth-child(1) {
  transform: rotate(45deg) translate(5px, 10px);
}

.menu-icon span.open:nth-child(2) {
  opacity: 0;
}

.menu-icon span.open:nth-child(3) {
  transform: rotate(-45deg) translate(5px, -10px);
}

/* Menu styles */
.menu {
  position: absolute;
  top: 40px;
  left: 0;
  background: #ffffff;
  border: 1px solid #ddd;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  width: 200px;
  overflow: hidden;
  max-height: 0;
  opacity: 0;
  transition:
    max-height 0.4s ease,
    opacity 0.4s ease;

  /* Show on top of all else */
  z-index: var(--z-index-top-most);
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

.menu li > a {
  display: flex;
  flex-direction: row;
  gap: 0.3rem;
  justify-content: left;
  align-content: center;
  align-items: center;
}

.menu li > a > span:first-child {
  min-width: 20%;
  display: flex;
  justify-content: center;
  padding: 0;
}

.menu li > a > span:first-child > span {
  font-size: 1.8rem;
}

.menu a {
  display: block;
  padding: 10px 20px;
  color: #333;
  text-decoration: none;
}

.menu a:hover {
  background: #0a5ef8;
  color: #fff;
}

/* Show menu '.menu' when menu-toggle is checked (and is immediately followed by an element */
.menu-toggle:checked + .menu-icon + .menu {
  max-height: 500px;
  /* large enough to fit all items */
  opacity: 1;
}
</style>
