import { createTrip, getSignalRInfo } from '@/api/trips';

export default {
  namespaced: true,

  state() {
    return {
      trip: {},
      currentStep: 0,
      contentLoading: false
    };
  },

  getters: {
    trip: state => state.trip,
    currentStep: state => state.currentStep,
    contentLoading: state => state.contentLoading
  },

  mutations: {
    trip(state, value) {
      state.trip = value;
    },
    currentStep(state, value) {
      state.currentStep = value;
    },
    contentLoading(state, value) {
      state.contentLoading = value;
    }
  },

  actions: {
    setTrip({ commit }, value) {
      commit('trip', value);
    },

    setCurrentStep({ commit }, value) {
      commit('currentStep', value);
    },

    async createTrip({ commit }, value) {
      try {
        commit('contentLoading', true);
        let trip = await createTrip(value);
        commit('trip', trip.data);
        return trip.data;
      } catch (e) {
        throw e;
      } finally {
        commit('contentLoading', false);
      }
    },

    async getSignalRInfo({ commit }, username) {
      try {
        let signalRInfo = await getSignalRInfo(username);
        return signalRInfo;
      } catch (e) {
        throw e;
      }
    }
  }
};
