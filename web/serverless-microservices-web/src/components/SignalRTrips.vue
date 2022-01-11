<template>
  <div></div>
</template>
<script>
import * as signalR from "@microsoft/signalr";
import { createNamespacedHelpers } from "vuex";
import { getPassenger } from "@/api/passengers";
const { mapGetters: commonGetters } = createNamespacedHelpers("common");
const {
  mapGetters: tripGetters,
  mapActions: tripActions
} = createNamespacedHelpers("trips");

export default {
  name: "SignalRTrips",
  data() {
    return {};
  },
  computed: {
    ...commonGetters(["notificationSystem", "user"]),
    ...tripGetters(["trip", "currentStep", "contentLoading"])
  },
  watch: {
    user(val, old) {
      // We watch for the username to change due to logging in or out.
      // If the username was previously null, but now isn't, then we
      // connect to SignalR Service.
      if (old === null && val !== null) {
        this.connectToSignalR();
      }
    }
  },
  methods: {
    ...tripActions([
      "setTrip",
      "setCurrentStep",
      "createTrip",
      "getSignalRInfo"
    ]),
    async getSignalRInformation() {
      let passengerInfo = await getPassenger(this.user.idTokenClaims.oid);
      if (passengerInfo && passengerInfo.data) {
        // Pass in the current user email so messages can be sent to just the user.
        let rawResponse = await this.getSignalRInfo(passengerInfo.data.email); // {url: '', status: 201};//await this.getSignalRInfo();
        if (rawResponse.status === 200) {
          let signalRInfo = rawResponse.data;
          console.log(`Connection Endpoint: ${signalRInfo.url}`);
          return signalRInfo;
        } else {
          console.log(`getSignalRInfo Response status: ${rawResponse.status}`);
          return null;
        }
      }
    },
    connectToSignalR() {
      if (this.user !== null) {
        this.getSignalRInformation()
          .then(signalrInfo => {
            if (signalrInfo !== null && signalrInfo !== undefined) {
              let options = {
                accessTokenFactory: () => signalrInfo.accessToken
              };

              let hubConnection = new signalR.HubConnectionBuilder()
                .withUrl(signalrInfo.url, options)
                .configureLogging(signalR.LogLevel.Information)
                .build();

              console.log("Connected to SignalR");

              hubConnection.on("tripUpdated", trip => {
                console.log(`tripUpdated Trip code: ${trip.code}`);
                this.$toast.success(
                  `Trip Code: ${trip.code}. Message: tripUpdated.`,
                  "Trip Updated",
                  this.notificationSystem.options.success
                );
              });

              hubConnection.on("tripDriversNotified", trip => {
                console.log(`tripDriversNotified Trip code: ${trip.code}`);
                this.$toast.info(
                  `Trip Code: ${trip.code}. Message: tripDriversNotified.`,
                  "Drivers Notified",
                  this.notificationSystem.options.info
                );
              });

              hubConnection.on("tripDriverPicked", trip => {
                console.log(`tripDriverPicked Trip code: ${trip.code}`);
                this.setCurrentStep(2);
                this.setTrip(trip);
                this.$toast.info(
                  `Trip Code: ${trip.code}. Message: tripDriverPicked.`,
                  "Driver Picked",
                  this.notificationSystem.options.info
                );
              });

              hubConnection.on("tripStarting", trip => {
                console.log(`tripStarting Trip code: ${trip.code}`);
                this.setCurrentStep(3);
                this.$toast.info(
                  `Trip Code: ${trip.code}. Message: tripStarting.`,
                  "Trip Starting",
                  this.notificationSystem.options.info
                );
              });

              hubConnection.on("tripRunning", trip => {
                console.log(`tripRunning Trip code: ${trip.code}`);
                if (this.currentStep < 3) {
                  this.setCurrentStep(3);
                }
                this.$toast.info(
                  `Trip Code: ${trip.code}. Message: tripRunning.`,
                  "Trip Running",
                  this.notificationSystem.options.info
                );
              });

              hubConnection.on("tripCompleted", trip => {
                console.log(`tripCompleted Trip code: ${trip.code}`);
                this.setCurrentStep(4);
                this.$toast.success(
                  `Trip Code: ${trip.code}. Message: tripCompleted.`,
                  "Trip Completed",
                  this.notificationSystem.options.success
                );
              });

              hubConnection.on("tripAborted", trip => {
                console.log(`tripAborted Trip code: ${trip.code}`);
                this.setCurrentStep(0);
                this.$toast.warning(
                  `Trip Code: ${trip.code}. Message: tripAborted.`,
                  "Trip Aborted",
                  this.notificationSystem.options.warning
                );
              });

              hubConnection.start().catch(err => console.log(err.toString()));
            } else {
              console.log("signalrInfo is null");
            }
          })
          .catch(err => {
            this.$toast.error(
              err.response ? err.response : err.message ? err.message : err,
              "Error",
              this.notificationSystem.options.error
            );
          });
      } else {
        console.log(
          "Not connecting to SignalR because the user is not authenticated."
        );
      }
    }
  },
  mounted() {
    this.connectToSignalR();
  }
};
</script>