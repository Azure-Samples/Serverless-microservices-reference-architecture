import Vue from 'vue';
import Vuex from 'vuex';
// import createPersistedState from 'vuex-persistedstate';

import common from './common';
import drivers from './drivers';
import passengers from './passengers';
import trips from './trips';

Vue.use(Vuex);

const store = new Vuex.Store({
  actions: {
    async init({ dispatch }) {
      // await dispatch('login')
    }
  },
  // plugins: [createPersistedState()],
  modules: {
    common,
    drivers,
    passengers,
    trips
  }
});

export default store;
