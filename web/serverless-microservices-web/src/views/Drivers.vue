<template>
  <div>
    <header class="masthead masthead-custom">
      <div class="container h-100" style="height:155px;">
        <div class="row justify-content-center h-100" style="height:120px;">
          <div
            class="col-12 col-lg-7 mt-auto"
            style="margin:0px;margin-top:0px;margin-bottom:0px;height:80px;max-width:100%;"
          >
            <div class="mx-auto header-content">
              <h1 class="mb-5">
                DRIVERS
                <img class="float-right" src="../assets/img/driver-icon.png" alt="Drivers">
              </h1>
            </div>
          </div>
        </div>
      </div>
    </header>
    <BlockUI message="Please wait..." :html="html" v-show="contentLoading"></BlockUI>
    <section id="features" class="features" style="padding-top:60px;">
      <div class="container">
        <div class="section-heading text-center" style="margin-bottom:50px;">
          <h2>View driver information</h2>
          <p class="text-muted">View all drivers and driver profile information</p>
          <hr>
          <div class="card border-danger">
            <div class="card-body">
              <h4 class="card-title">All Drivers</h4>
              <b-table
                show-empty
                responsive
                striped
                hover
                :items="drivers"
                :fields="fields"
                :current-page="currentPage"
                :per-page="perPage"
              >
                <template slot="code" slot-scope="row">
                  <strong>{{row.value}}</strong>
                </template>
                <template slot="firstName" slot-scope="row">{{row.value}}</template>
                <template slot="lastName" slot-scope="row">{{row.value}}</template>
                <template slot="latitude" slot-scope="row">{{row.value}}</template>
                <template slot="longitude" slot-scope="row">{{row.value}}</template>
                <template slot="isAcceptingRides" slot-scope="row">{{row.value?'Yes':'No'}}</template>
                <template slot="actions" slot-scope="row">
                  <!-- We use @click.stop here to prevent a 'row-clicked' event from also happening -->
                  <b-button size="sm" @click.stop="selectDriver(row.item)" class="mr-1">Select</b-button>
                </template>
              </b-table>
            </div>
          </div>
        </div>
      </div>
      <div class="container">
        <div class="section-heading text-center" style="margin-bottom:50px;">
          <h2>Predict the best pickup location</h2>
          <p
            class="text-muted"
          >Use our advanced AI to predict the best place to pickup passengers any given day</p>
        </div>
        <div class="container">
          <div class="row">
            <div class="col-lg-6 my-auto">
              <div class="device-container">
                <div class="device-container">
                  <div class="text-center">
                    <img class="img-fluid" src="../assets/img/calendar.png" alt="Calendar">
                  </div>
                </div>
                <p class="text-muted" style="margin-top:28px;font-size:16px;">
                  Our specially trained AI predicts the best location for you to pick up the most fares, any day of the week.
                  It has been trained to take into account historical trips and ride trends for each location by day of the week and week of the month.
                  Simply select the day and our glorious robot will tell you where to hang out to make the most cash! It's a win-win for all involved :)
                </p>
              </div>
            </div>
            <div class="col-lg-6">
              <div class="device-container">
                <div>
                  <div class="card text-black bg-warning">
                    <div class="card-header">PREDICT BEST PICKUP LOCATION</div>
                    <div class="card-body text-left">
                      <p>Select any date to predict where the best place to wait for fares will be.</p>
                      <b-form-row>
                        <b-col>
                          <em>
                            <strong>Date</strong>
                          </em>
                        </b-col>
                        <b-col>
                          <b-form-input type="date" v-model="predictionDate"></b-form-input>
                        </b-col>
                      </b-form-row>
                      <hr>
                      <div class="card text-white bg-secondary" v-if="prediction">
                        <div class="card-header">PREDICTION</div>
                        <div class="card-body">
                          <b-form-row>
                            <b-col>
                              <em>
                                <strong>{{prediction.locationFriendlyName}}</strong>
                              </em>
                            </b-col>
                            <b-col>{{prediction.location.latitude}}, {{prediction.location.longitude}}</b-col>
                          </b-form-row>
                        </div>
                      </div>
                    </div>
                    <div class="card-footer">
                      <b-btn size="sm" class="float-right mr-1" @click="requestPrediction()">Submit</b-btn>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="row">
            <div class="col-lg-12">
              <GmapMap
                :center="{ lat: 47.618282, lng: -122.241035 }"
                :zoom="11"
                map-type-id="terrain"
                style="width: 100%; height: 350px"
                ref="mapRef"
              >
                <GmapInfoWindow
                  :options="infoOptions"
                  :position="infoWindowPos"
                  :opened="infoWinOpen"
                  @closeclick="infoWinOpen = false;"
                >{{ infoContent }}</GmapInfoWindow>
                <GmapMarker
                  :key="index"
                  v-for="(m, index) in markers"
                  :title="m.infoText"
                  :position="m.position"
                  :clickable="true"
                  :draggable="true"
                  @click="toggleInfoWindow(m, index);"
                ></GmapMarker>
              </GmapMap>
            </div>
          </div>
        </div>
      </div>
    </section>
    <!-- Driver information modal -->
    <b-modal
      id="driverInformationModal"
      ref="modalRef"
      title="DRIVER INFORMATION"
      header-bg-variant="warning"
      header-text-variant="dark"
      footer-bg-variant="light"
      footer-text-variant="dark"
    >
      <b-container fluid>
        <b-form-row>
          <b-col>
            <em>
              <strong>Code</strong>
            </em>
          </b-col>
          <b-col>{{selectedDriver.code}}</b-col>
        </b-form-row>
        <b-form-row>
          <b-col>
            <em>
              <strong>First name</strong>
            </em>
          </b-col>
          <b-col>{{selectedDriver.firstName}}</b-col>
        </b-form-row>
        <b-form-row>
          <b-col>
            <em>
              <strong>Last name</strong>
            </em>
          </b-col>
          <b-col>{{selectedDriver.lastName}}</b-col>
        </b-form-row>
        <b-form-row>
          <b-col>
            <em>
              <strong>Coordinates</strong>
            </em>
          </b-col>
          <b-col>{{selectedDriver.latitude}}, {{selectedDriver.longitude}}</b-col>
        </b-form-row>
        <b-form-row>
          <b-col>
            <em>
              <strong>Accepting rides?</strong>
            </em>
          </b-col>
          <b-col>{{selectedDriver.isAcceptingRides?'Yes':'No'}}</b-col>
        </b-form-row>
        <hr>
        <div class="card text-white bg-secondary">
          <div class="card-header">CAR</div>
          <div class="card-body">
            <b-form-row>
              <b-col>
                <em>
                  <strong>Make</strong>
                </em>
              </b-col>
              <b-col>{{selectedDriver.car.make}}</b-col>
            </b-form-row>
            <b-form-row>
              <b-col>
                <em>
                  <strong>Model</strong>
                </em>
              </b-col>
              <b-col>{{selectedDriver.car.model}}</b-col>
            </b-form-row>
            <b-form-row>
              <b-col>
                <em>
                  <strong>Color</strong>
                </em>
              </b-col>
              <b-col>{{selectedDriver.car.color}}</b-col>
            </b-form-row>
            <b-form-row>
              <b-col>
                <em>
                  <strong>License plate</strong>
                </em>
              </b-col>
              <b-col>{{selectedDriver.car.licensePlate}}</b-col>
            </b-form-row>
          </div>
        </div>
      </b-container>
      <div slot="modal-footer" class="w-100">
        <p class="float-left"></p>
        <b-btn size="sm" class="float-right" variant="primary" @click="hideModal()">Close</b-btn>
      </div>
    </b-modal>
  </div>
</template>

<script>
import { createNamespacedHelpers } from "vuex";
import moment from "moment";
const { mapGetters: commonGetters } = createNamespacedHelpers("common");
const {
  mapGetters: driverGetters,
  mapActions: driverActions
} = createNamespacedHelpers("drivers");

export default {
  name: "Drivers",
  props: ["authenticated"],
  data() {
    return {
      drivers: [],
      driverInfo: null,
      predictionDate: moment().format("YYYY-MM-DD"),
      prediction: null,
      html: '<i class="fas fa-cog fa-spin fa-3x fa-fw"></i>',
      fields: [
        { key: "code", label: "Code", sortable: true },
        { key: "firstName", label: "First Name", sortable: true },
        {
          key: "lastName",
          label: "Last Name",
          sortable: true
        },
        { key: "latitude", label: "Latitude", class: "text-right" },
        { key: "longitude", label: "Longitude", class: "text-right" },
        {
          key: "isAcceptingRides",
          label: "Accepting rides?",
          class: "text-right"
        },
        { key: "actions", label: "" }
      ],
      currentPage: 1,
      perPage: 10,
      pageOptions: [5, 10, 15],
      infoContent: "",
      infoWindowPos: null,
      infoWinOpen: false,
      currentMidx: null,
      //optional: offset infowindow so it visually sits nicely on top of our marker
      infoOptions: {
        pixelOffset: {
          width: 0,
          height: -35
        }
      },
      markers: [],
      heatMapData: []
    };
  },
  computed: {
    ...commonGetters(["notificationSystem", "pickUpLocations"]),
    ...driverGetters(["selectedDriver", "contentLoading"])
  },
  methods: {
    ...driverActions(["getDrivers", "setSelectedDriver", "predict"]),
    retrieveDrivers() {
      this.getDrivers()
        .then(response => {
          this.drivers = response;
        })
        .catch(err => {
          this.$toast.error(
            err.response ? err.response : err.message ? err.message : err,
            "Error",
            this.notificationSystem.options.error
          );
        });
    },
    selectDriver(driver) {
      this.setSelectedDriver(driver);
      this.$refs.modalRef.show();
    },
    hideModal() {
      this.$refs.modalRef.hide();
    },
    requestPrediction() {
      this.predict(this.predictionDate)
        .then(response => {
          if (response.topPrediction) {
            this.prediction = response.topPrediction;
            if (this.pickUpLocations) {
              // See if we can match the location to a known location for the friendly name.
              let known = this.pickUpLocations.filter(el => {
                return (
                  el.latitude === this.prediction.location.latitude &&
                  el.longitude === this.prediction.location.longitude
                );
              });
              if (known) {
                this.prediction.locationFriendlyName = known[0].name;
              } else {
                this.prediction.locationFriendlyName = "Here";
              }
            }
          }

          if (response.allPredictions) {
            // Create markers and heatmaps for all predictions.
            this.markers = [];
            this.heatMapData = [];
            for (var i = 0; i < response.allPredictions.length; i++) {
              let pred = response.allPredictions[i];
              this.markers.push({
                infoText: Math.round(pred.predictedPickupRequests).toString(),
                position: {
                  lat: pred.location.latitude,
                  lng: pred.location.longitude
                }
              });
              this.heatMapData.push({
                location: new google.maps.LatLng(
                  pred.location.latitude,
                  pred.location.longitude
                ),
                weight: pred.predictedPickupRequests
              });
            }

            // At this point, the child GmapMap has been mounted, but
            // its map has not been initialized.
            // Therefore we need to write mapRef.$mapPromise.then(() => ...)
            this.$refs.mapRef.$mapPromise.then(map => {
              var heatmap = new google.maps.visualization.HeatmapLayer({
                data: this.heatMapData,
                map: map,
                radius: 20
              });
            });
          }
        })
        .catch(err => {
          this.$toast.error(
            err.response ? err.response : err.message ? err.message : err,
            "Error",
            this.notificationSystem.options.error
          );
        });
    },
    toggleInfoWindow: function(marker, idx) {
      this.infoWindowPos = marker.position;
      this.infoContent = marker.infoText;

      //check if its the same marker that was selected if yes toggle
      if (this.currentMidx == idx) {
        this.infoWinOpen = !this.infoWinOpen;
      }
      //if different marker set infowindow to open and reset current marker index
      else {
        this.infoWinOpen = true;
        this.currentMidx = idx;
      }
    }
  },
  mounted() {
    this.retrieveDrivers();
  }
};
</script>