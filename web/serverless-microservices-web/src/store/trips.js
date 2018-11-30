import { createTrip, submitTripReview } from '@/api/trips';

export default {
  namespaced: true,

  state() {
    return {
      trip: {},
      currentStep: 0,
      reviewResults: {},
      contentLoading: false
    };
  },

  getters: {
    trip: state => state.trip,
    currentStep: state => state.currentStep,
    reviewResults: state => state.reviewResults,
    contentLoading: state => state.contentLoading
  },

  mutations: {
    trip(state, value) {
      state.trip = value;
    },
    currentStep(state, value) {
      state.currentStep = value;
    },
    reviewResults(state, value) {
      state.reviewResults = value;
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

    clearReviewResults({ commit }) {
      commit('reviewResults', {});
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

    async submitTripReview({ commit }, payload) {
      try {
        commit('contentLoading', true);
        let value = {
          driverRating: payload.rating,
          review: payload.review
        };
        let results = await submitTripReview(
          payload.code,
          payload.driverCode,
          value
        );
        commit('reviewResults', results.data);
        return results.data;
      } catch (e) {
        throw e;
      } finally {
        commit('contentLoading', false);
      }
    }
  }
};
