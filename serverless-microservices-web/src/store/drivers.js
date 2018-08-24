import { getDrivers } from '@/api/drivers';

export default {
  namespaced: true,

  state() {
    return {
      drivers: null,
      selectedDriver: { car: {} },
      contentLoading: false
    };
  },

  getters: {
    drivers: state => state.drivers,
    selectedDriver: state => state.selectedDriver,
    contentLoading: state => state.contentLoading
  },

  mutations: {
    selectedDriver(state, value) {
      state.selectedDriver = value;
    },
    drivers(state, value) {
      state.drivers = value;
    },
    contentLoading(state, value) {
      state.contentLoading = value;
    }
  },

  actions: {
    setSelectedDriver({ commit }, value) {
      commit('selectedDriver', value);
    },

    async getDrivers({ commit }) {
      try {
        commit('contentLoading', true);
        let drivers = await getDrivers();
        return drivers.data;
      } catch (e) {
        throw e;
      } finally {
        commit('contentLoading', false);
      }
    }
  }
};
