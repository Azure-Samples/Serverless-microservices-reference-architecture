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
              <h1 class="mb-5">MY TRIP
                <img class="float-right" src="../assets/img/ride-icon.png" alt="Start my ride">
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
          <h2>Find my ride!</h2>
          <p class="text-muted">Confirm your pickup and destination to start your trip.</p>
          <hr>
        </div>
        <div class="row">
          <div class="container-fluid">
            <div class="row">
              <div class="col-md-4">
                <div class="feature-item">
                  <i class="icon-location-pin text-primary"></i>
                  <h3>Confirm pickup location</h3>
                </div>
                <div class="feature-item">
                  <b-dropdown id="ddown-pickup" text="Pickup Location" variant="info" class>
                    <b-dropdown-item-button
                      @click.stop="selectPickup(location)"
                      v-bind:key="location.id"
                      v-for="location in pickUpLocations"
                    >{{location.name}}</b-dropdown-item-button>
                  </b-dropdown>
                  <div v-show="selectedPickUpLocationName !== null">{{ selectedPickUpLocationName }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="feature-item">
                  <i class="icon-map text-primary"></i>
                  <h3>Select destination</h3>
                </div>
                <div class="feature-item">
                  <b-dropdown id="ddown-pickup" text="Destination" variant="info" class>
                    <b-dropdown-item-button
                      @click.stop="selectDestination(location)"
                      v-bind:key="location.id"
                      v-for="location in destinationLocations"
                    >{{location.name}}</b-dropdown-item-button>
                  </b-dropdown>
                  <div
                    v-show="selectedDestinationLocationName !== null"
                  >{{ selectedDestinationLocationName }}</div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="feature-item">
                  <i class="icon-flag text-primary"></i>
                  <h3>Request a driver</h3>
                </div>
                <div class="feature-item">
                  <b-button
                    id="request-driver"
                    text="Request Driver"
                    variant="primary"
                    v-bind:disabled="requestDriverDisabled"
                    @click="requestDriver()"
                  >Request Driver</b-button>
                  <p v-show="requestDriverDisabled" class="text-muted">
                    <em>Select your pickup location and destination to start your trip</em>
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
        <hr>
        <!-- <div class="section-heading text-center" style="margin-bottom:50px;">
                <h2>Trip progress</h2>
                <hr>
        </div>-->
        <ol class="step-indicator" key="trip-progress">
          <li :class="tripRequestedClass">
            <div class="step">
              <i class="fas fa-check"></i>
            </div>
            <div class="caption hidden-xs hidden-sm">Trip requested</div>
          </li>
          <li :class="driverFoundClass">
            <div class="step">
              <i class="fas fa-car"></i>
            </div>
            <div class="caption hidden-xs hidden-sm">Driver found</div>
          </li>
          <li :class="tripStartedClass">
            <div class="step">
              <i class="fas fa-car-side"></i>
            </div>
            <div class="caption hidden-xs hidden-sm">Trip started</div>
          </li>
          <li :class="tripEndedClass">
            <div class="step">
              <i class="fas fa-flag-checkered"></i>
            </div>
            <div class="caption hidden-xs hidden-sm">You have arrived!</div>
          </li>
        </ol>
      </div>
    </section>
    <section id="features" class="features" style="padding-top:10px;">
      <div class="container">
        <div class="row">
          <div class="col-lg-6 my-auto" v-if="currentStep > 0">
            <div class="container-fluid" v-if="driverFound">
              <div class="row">
                <div class="col-lg-6">
                  <div class="feature-item">
                    <i class="icon-user text-primary"></i>
                    <h3>Your driver is {{trip.driver.firstName}} {{trip.driver.lastName}}</h3>
                    <p class="text-muted">Meet {{trip.driver.firstName}} at the pickup point.</p>
                  </div>
                </div>
                <div class="col-lg-6">
                  <div class="feature-item">
                    <div class="card text-black bg-warning">
                      <div class="card-header">CAR</div>
                      <div class="card-body">
                        <b-form-row>
                          <b-col>
                            <em>
                              <strong>Make</strong>
                            </em>
                          </b-col>
                          <b-col>{{trip.driver.car.make}}</b-col>
                        </b-form-row>
                        <b-form-row>
                          <b-col>
                            <em>
                              <strong>Model</strong>
                            </em>
                          </b-col>
                          <b-col>{{trip.driver.car.model}}</b-col>
                        </b-form-row>
                        <b-form-row>
                          <b-col>
                            <em>
                              <strong>Color</strong>
                            </em>
                          </b-col>
                          <b-col>{{trip.driver.car.color}}</b-col>
                        </b-form-row>
                        <b-form-row>
                          <b-col>
                            <em>
                              <strong>License plate</strong>
                            </em>
                          </b-col>
                          <b-col>{{trip.driver.car.licensePlate}}</b-col>
                        </b-form-row>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="container-fluid" v-else>
              <div class="feature-item">
                <i class="icon-user text-primary"></i>
                <h3>Searching for a nearby driver...</h3>
                <p class="text-muted">Your driver's information will appear here once found.</p>
              </div>
            </div>
          </div>
          <div class="col-lg-6 my-auto" v-else>
            <div class="container-fluid">
              <div class="feature-item">
                <i class="icon-screen-smartphone text-primary"></i>
                <h3>Use the options above to find a ride</h3>
                <p class="text-muted">
                  Confirm your pickup location, set your destination, then select
                  <strong>request driver</strong>
                  to get started. Your driver information will appear here once they are found!
                </p>
              </div>
            </div>
          </div>
          <div class="col-lg-6 my-auto">
            <div class="device-container">
              <div class="device-container">
                <div>
                  <img class="img-fluid" src="../assets/img/yellow-car.png" alt="Yellow car">
                </div>
              </div>
              <p
                class="text-muted"
                style="margin-top:28px;font-size:16px;"
              >Our drivers pass rigorous background checks and are required to maintain a high standard of driving, customer satisfaction, and vehicle maintenance. You will be rolling like a rockstar in no time!</p>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script>
import { createNamespacedHelpers } from "vuex";
const {
  mapGetters: commonGetters,
  mapActions: commonActions
} = createNamespacedHelpers("common");
import { getDrivers, getDriver } from "@/api/drivers";
import { getPassenger } from "@/api/passengers";
import { Authentication } from "@/utils/Authentication";
const {
  mapGetters: tripGetters,
  mapActions: tripActions
} = createNamespacedHelpers("trips");

const auth = new Authentication();

export default {
  name: "Trip",
  props: ["authenticated"],
  data() {
    return {
      drivers: [],
      selectedDriver: null,
      selectedPickUpLocation: null,
      selectedDestinationLocation: null,
      driverInfo: null,
      passengerInfo: null,
      html: '<i class="fas fa-cog fa-spin fa-3x fa-fw"></i>'
    };
  },
  computed: {
    ...commonGetters([
      "notificationSystem",
      "user",
      "pickUpLocations",
      "destinationLocations"
    ]),
    ...tripGetters(["trip", "currentStep", "contentLoading"]),
    requestDriverDisabled() {
      return (
        this.selectedPickUpLocation === null ||
        this.selectedDestinationLocation === null
      );
    },
    selectedPickUpLocationName() {
      return this.selectedPickUpLocation !== null &&
        this.selectedPickUpLocation !== undefined
        ? this.selectedPickUpLocation.name
        : null;
    },
    selectedDestinationLocationName() {
      return this.selectedDestinationLocation !== null &&
        this.selectedDestinationLocation !== undefined
        ? this.selectedDestinationLocation.name
        : null;
    },
    driverFound() {
      return (
        this.trip !== undefined &&
        this.trip !== null &&
        this.trip.driver !== undefined &&
        this.trip.driver !== null
      );
    },
    tripRequestedClass() {
      return {
        active: this.currentStep === 1,
        complete: this.currentStep > 1
      };
    },
    driverFoundClass() {
      return {
        active: this.currentStep === 2,
        complete: this.currentStep > 2
      };
    },
    tripStartedClass() {
      return {
        active: this.currentStep === 3,
        complete: this.currentStep > 3
      };
    },
    tripEndedClass() {
      return {
        active: this.currentStep === 4,
        complete: this.currentStep > 4
      };
    }
  },
  methods: {
    ...commonActions(["setUser"]),
    ...tripActions(["setTrip", "setCurrentStep", "createTrip"]),
    createTripRequest(trip) {
      this.createTrip(trip)
        .then(response => {
          this.setCurrentStep(1);
          this.$toast.success(
            `Request Code: <b>${response.code}`,
            "Driver Requested Successfully",
            this.notificationSystem.options.success
          );
        })
        .catch(err => {
          console.error(err)
          this.$toast.error(
            err.response ? err.response.data : err.message ? err.message : err,
            "Error",
            this.notificationSystem.options.error
          );
        });
    },
    requestDriver() {
      if (this.user) {
        if (this.user.idTokenClaims === undefined || this.user.idTokenClaims.emails === undefined || this.user.idTokenClaims.emails.length == 0) {
          console.error("this.user.idTokenClaims.emails is not present or does not contain any emails. Please check user claims and permissions.")
          this.$toast.error(
            "User's email not found",
            "Error",
            this.notificationSystem.options.error
          );
        }

        var trip = {
          passenger: {
            code: this.user.idTokenClaims.emails[0],
            firstName: this.user.idTokenClaims.given_name,
            surname: this.user.idTokenClaims.family_name,
            //"mobileNumber": this.passengerInfo.mobileNumber,
            email: this.user.idTokenClaims.emails[0]
          },
          source: {
            latitude: this.selectedPickUpLocation.latitude,
            longitude: this.selectedPickUpLocation.longitude
          },
          destination: {
            latitude: this.selectedDestinationLocation.latitude,
            longitude: this.selectedDestinationLocation.longitude
          },
          type: 1 //0 = Normal, 1 = Demo
        };
        this.createTripRequest(trip);
      } else {
        this.$toast.error(
          "You must be logged in to start a new trip!",
          "Error",
          this.notificationSystem.options.error
        );
      }
    },
    selectPickup(location) {
      this.selectedPickUpLocation = location;
    },
    selectDestination(location) {
      this.selectedDestinationLocation = location;
    }
  },
  mounted() {
    // if (!this.user) {
    //   auth.login().then(
    //     user => {
    //       if (user) {
    //         this.setUser(user);
    //       } else {
    //         this.setUser(null);
    //       }
    //     },
    //     () => {
    //       this.setUser(null);
    //     }
    //   );
    // }
  }
};
</script>

<style scoped>
section.features .feature-item {
  padding-top: 0px;
  padding-bottom: 0px;
  text-align: center;
}

.step-indicator {
  border-collapse: separate;
  display: table;
  margin-left: 0px;
  position: relative;
  table-layout: fixed;
  text-align: center;
  vertical-align: middle;
  padding-left: 0;
  padding-top: 20px;
}
.step-indicator li {
  display: table-cell;
  position: relative;
  float: none;
  padding: 0;
  width: 1%;
}
.step-indicator li:after {
  background-color: #ccc;
  content: "";
  display: block;
  height: 1px;
  position: absolute;
  width: 100%;
  top: 32px;
}
.step-indicator li:after {
  left: 50%;
}
.step-indicator li:last-child:after {
  display: none;
}
.step-indicator li.active .step {
  border-color: #007bff;
  color: #007bff;
}
.step-indicator li.active .caption {
  color: #007bff;
}
.step-indicator li.complete:after {
  /* background-color: #fdcc52; */
  background: #339933;
}
.step-indicator li.complete .step {
  /* border-color: #fdcc52; */
  color: #339933;
  border-color: #339933;
}
.step-indicator li.complete .caption {
  color: #339933;
}
.step-indicator .step {
  background-color: #fff;
  border-radius: 50%;
  border: 1px solid #ccc;
  color: #ccc;
  font-size: 24px;
  height: 64px;
  line-height: 64px;
  margin: 0 auto;
  position: relative;
  width: 64px;
  z-index: 1;
}
.step-indicator .step:hover {
  cursor: pointer;
}
.step-indicator .caption {
  color: #ccc;
  padding: 11px 16px;
}
</style>