export default {
  namespaced: true,

  state() {
    return {
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
    notificationSystem: state => state.notificationSystem
  },

  mutations: {
    notificationSystem(state, value) {
      state.notificationSystem = value;
    }
  },

  actions: {}
};
