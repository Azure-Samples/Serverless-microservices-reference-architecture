<template>
</template>

<script>
import * as signalR from '@aspnet/signalr';
import { createNamespacedHelpers } from 'vuex';
const { mapGetters: commonGetters } = createNamespacedHelpers('common');
const {
  mapGetters: tripGetters,
  mapActions: tripActions
} = createNamespacedHelpers('trips');

export default {
  name: 'SignalRTrips',
  data() {
    return {};
  },
  computed: {
    ...commonGetters(['notificationSystem']),
    ...tripGetters(['trip', 'currentStep', 'contentLoading'])
  },
  methods: {
    ...tripActions(['setTrip', 'setCurrentStep', 'createTrip']),
    getSignalRInfo: async url => {
      console.log(`SignalR Info URL ${url}`);
      let rawResponse = await fetch(url, {
        method: 'GET', // *GET, POST, PUT, DELETE, etc.
        mode: 'cors', // no-cors, cors, *same-origin
        cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
        credentials: 'same-origin', // include, same-origin, *omit
        headers: {
          'Content-Type': 'application/json; charset=utf-8'
        },
        redirect: 'follow', // manual, *follow, error
        referrer: 'no-referrer' // no-referrer, *client
      });
      if (rawResponse.status === 200) {
        let signalRInfo = await rawResponse.json();
        console.log(`Connection Endpoint: ${signalRInfo.endpoint}`);
        return signalRInfo;
      } else {
        console.log(`getSignalRInfo Response status: ${rawResponse.status}`);
        throw `Could not obtain SignalR info. Response was ${
          rawResponse.status
        }`;
        return null;
      }
    },
    connectToSignalR() {
      this.getSignalRInfo(window.signalrInfoUrl)
        .then(signalrInfo => {
          if (signalrInfo !== null && signalrInfo !== undefined) {
            let options = {
              accessTokenFactory: () => signalrInfo.accessKey
            };

            let hubConnection = new signalR.HubConnectionBuilder()
              .withUrl(signalrInfo.endpoint, options)
              .configureLogging(signalR.LogLevel.Information)
              .build();

            console.log('Connected to SignalR');

            hubConnection.on('tripUpdated', trip => {
              console.log(`tripUpdated Trip code: ${trip.code}`);
              this.$toast.success(
                `Trip Code: ${trip.code}. Message: tripUpdated.`,
                'Trip Updated',
                this.notificationSystem.options.success
              );
            });

            hubConnection.on('tripDriversNotified', trip => {
              console.log(`tripDriversNotified Trip code: ${trip.code}`);
              this.setCurrentStep(1);
              this.$toast.info(
                `Trip Code: ${trip.code}. Message: tripDriversNotified.`,
                'Drivers Notified',
                this.notificationSystem.options.info
              );
            });

            hubConnection.on('tripDriverPicked', trip => {
              console.log(`tripDriverPicked Trip code: ${trip.code}`);
              this.setCurrentStep(2);
              this.$toast.info(
                `Trip Code: ${trip.code}. Message: tripDriverPicked.`,
                'Driver Picked',
                this.notificationSystem.options.info
              );
            });

            hubConnection.on('tripStarting', trip => {
              console.log(`tripStarting Trip code: ${trip.code}`);
              this.setCurrentStep(1);
              this.$toast.info(
                `Trip Code: ${trip.code}. Message: tripStarting.`,
                'Trip Starting',
                this.notificationSystem.options.info
              );
            });

            hubConnection.on('tripRunning', trip => {
              console.log(`tripRunning Trip code: ${trip.code}`);
              this.setCurrentStep(3);
              this.$toast.info(
                `Trip Code: ${trip.code}. Message: tripRunning.`,
                'Trip Running',
                this.notificationSystem.options.info
              );
            });

            hubConnection.on('tripCompleted', trip => {
              console.log(`tripCompleted Trip code: ${trip.code}`);
              this.setCurrentStep(4);
              this.$toast.success(
                `Trip Code: ${trip.code}. Message: tripCompleted.`,
                'Trip Completed',
                this.notificationSystem.options.success
              );
            });

            hubConnection.on('tripAborted', trip => {
              console.log(`tripAborted Trip code: ${trip.code}`);
              this.setCurrentStep(0);
              this.$toast.warning(
                `Trip Code: ${trip.code}. Message: tripAborted.`,
                'Trip Aborted',
                this.notificationSystem.options.warning
              );
            });

            hubConnection.start().catch(err => console.log(err.toString()));
          } else {
            console.log('signalrInfo is null');
          }
        })
        .catch(err => {
          this.$toast.error(
            err.response ? err.response : err.message ? err.message : err,
            'Error',
            this.notificationSystem.options.error
          );
        });
    }
  },
  mounted() {
    this.connectToSignalR();
  }
};
</script>