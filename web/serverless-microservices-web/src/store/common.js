export default {
  namespaced: true,

  state() {
    return {
      user: null,
      notificationSystem: {
        options: {
          info: {
            position: 'topRight'
          },
          success: {
            position: 'topRight'
          },
          warning: {
            position: 'topRight'
          },
          error: {
            position: 'topRight'
          }
        }
      }
    };
  },

  getters: {
    user: state => state.user,
    notificationSystem: state => state.notificationSystem
  },

  mutations: {
    user(state, value) {
      state.user = value;
    },
    notificationSystem(state, value) {
      state.notificationSystem = value;
    }
  },

  actions: {
    setUser({ commit }, value) {
      commit('user', value);
    }
  }
};
