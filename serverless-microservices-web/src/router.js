import Vue from 'vue';
import Router from 'vue-router';
import Home from '@/views/Home.vue';
import Trip from '@/views/Trip.vue';
import Passengers from '@/views/Passengers.vue';
import Drivers from '@/views/Drivers.vue';
import Login from '@/views/Login.vue';

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
      path: '/passengers',
      name: 'passengers',
      component: Passengers
    },
    {
      path: '/drivers',
      name: 'drivers',
      component: Drivers
    },
    {
      path: '/login',
      name: 'login',
      component: Login
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
