export default {
  namespaced: true,

  state () {
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
      },
      pickUpLocations: [
        {
          id: 1,
          name: 'Microsoft Corporate Office',
          latitude: 47.6423354,
          longitude: -122.1391189
        },
        {
          id: 2,
          name: 'Hyatt Regency Bellevue',
          latitude: 47.618282,
          longitude: -122.201035
        },
        {
          id: 3,
          name: 'Space Needle',
          latitude: 47.62053,
          longitude: -122.3493
        }
      ],
      destinationLocations: [
        {
          id: 1,
          name: 'Seattle, Washington',
          latitude: 47.6131746,
          longitude: -122.4821466
        },
        {
          id: 2,
          name: 'Bellevue, Washington',
          latitude: 47.5963256,
          longitude: -122.1928181
        },
        {
          id: 3,
          name: 'Redmond, Washington',
          latitude: 47.6721228,
          longitude: -122.1356409
        }
      ]
    }
  },

  getters: {
    user: state => state.user,
    notificationSystem: state => state.notificationSystem,
    pickUpLocations: state => state.pickUpLocations,
    destinationLocations: state => state.destinationLocations
  },

  mutations: {
    user (state, value) {
      state.user = value
    },
    notificationSystem (state, value) {
      state.notificationSystem = value
    },
    pickUpLocations (state, value) {
      state.pickUpLocations = value
    },
    destinationLocations (state, value) {
      state.destinationLocations = value
    }
  },

  actions: {
    setUser ({ commit }, value) {
      commit('user', value)
    }
  }
}
