import { createRouter, createWebHistory, type RouterOptions } from 'vue-router';
import HomeView from '../views/HomeView.vue';
import DevicesView from '../views/DevicesView.vue';
import MqttView from '../views/MqttView.vue';
import type { NavItem } from '@/models/app-models';

export const routes: NavItem[] = [
  {
    path: '/',
    name: 'home',
    label: 'Home',
    component: HomeView,
    enabled: true,
    icon: 'home'
  },
  {
    path: '/devices',
    name: 'devices',
    label: 'Devices',
    component: DevicesView,
    enabled: true,
    icon: 'developer_board'
  },
  {
    path: '/mqtt',
    name: 'mqtt',
    label: 'MQTT',
    component: MqttView,
    enabled: true,
    icon: 'tag'
  }
];

const routerOptions: RouterOptions = {
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: routes
};

const router = createRouter(routerOptions);

export default router;
