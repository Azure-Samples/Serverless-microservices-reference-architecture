import { getPassengers } from '@/api/passengers';

export default {
  namespaced: true,

  state() {
    return {
      passengers: null,
      selectedPassenger: {},
      contentLoading: false
    };
  },

  getters: {
    passengers: state => state.passengers,
    selectedPassenger: state => state.selectedPassenger,
    contentLoading: state => state.contentLoading
  },

  mutations: {
    selectedPassenger(state, value) {
      state.selectedPassenger = value;
    },
    passengers(state, value) {
      state.passengers = value;
    },
    contentLoading(state, value) {
      state.contentLoading = value;
    }
  },

  actions: {
    setSelectedPassenger({ commit }, value) {
      commit('selectedPassenger', value);
    },

    async getPassengers({ commit }) {
      try {
        commit('contentLoading', true);
        let passengers = await getPassengers();
        return passengers.data;
      } catch (e) {
        throw e;
      } finally {
        commit('contentLoading', false);
      }
    }
  }
};
