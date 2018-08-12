import Vue from 'vue';
import Router from 'vue-router';
import Home from '@/views/Home.vue';
import Trip from '@/views/Trip.vue';
import Riders from '@/views/Riders.vue';
import Drivers from '@/views/Drivers.vue';

Vue.use(Router);

export default new Router({
  mode: 'history',
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/trip',
      name: 'trip',
      component: Trip
    },
    {
      path: '/riders',
      name: 'riders',
      component: Riders
    },
    {
      path: '/drivers',
      name: 'drivers',
      component: Drivers
    }
  ],
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    }
    if (to.hash) {
      return { selector: to.hash };
    }
    return { x: 0, y: 0 };
  }
});
