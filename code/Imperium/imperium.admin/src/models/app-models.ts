import type { RouteComponent } from 'vue-router';

export interface MenuItem {
  icon?: string;
}

export interface AppRoute {
  name: string;
  component: RouteComponent;
}

export interface NavItem extends MenuItem, AppRoute {
  label: string;
  enabled: boolean;
  path: string;
}
