<template>
  <div>
    <header class="masthead masthead-custom">
        <div class="container h-100" style="height:155px;">
            <div class="row justify-content-center h-100" style="height:120px;">
                <div class="col-12 col-lg-7 mt-auto" style="margin:0px;margin-top:0px;margin-bottom:0px;height:80px;max-width:100%;">
                    <div class="mx-auto header-content">
                        <h1 class="mb-5">MY TRIP<img class="float-right" src="../assets/img/ride-icon.png" alt="Start my ride"></h1>
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
                            <div class="feature-item"><i class="icon-location-pin text-primary"></i>
                                <h3>Confirm pickup location</h3>
                            </div>
                            <div class="feature-item">
                                <b-dropdown id="ddown-pickup" text="Pickup Location" variant="info" class="">
                                    <b-dropdown-item-button @click.stop="selectPickup(location)" v-bind:key ="location.id" v-for="location in pickUpLocations">{{location.name}}</b-dropdown-item-button>
                                </b-dropdown>
                                <div v-show="selectedPickUpLocationName !== null">
                                    {{ selectedPickUpLocationName }}
                                </div>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="feature-item"><i class="icon-map text-primary"></i>
                                <h3>Select destination</h3>
                            </div>
                            <div class="feature-item">
                                <b-dropdown id="ddown-pickup" text="Destination" variant="info" class="">
                                    <b-dropdown-item-button @click.stop="selectDestination(location)" v-bind:key ="location.id" v-for="location in destinationLocations">{{location.name}}</b-dropdown-item-button>
                                </b-dropdown>
                                <div v-show="selectedDestinationLocationName !== null">
                                    {{ selectedDestinationLocationName }}
                                </div>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="feature-item"><i class="icon-flag text-primary"></i>
                                <h3>Request a driver</h3>
                            </div>
                            <div class="feature-item">
                                <b-button id="request-driver" text="Request Driver" variant="primary" v-bind:disabled="requestDriverDisabled" @click="requestDriver()">
                                    Request Driver
                                </b-button>
                                <p v-show="requestDriverDisabled" class="text-muted"><em>Select your pickup location and destination to start your trip</em></p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <hr>
            <!-- <div class="section-heading text-center" style="margin-bottom:50px;">
                <h2>Trip progress</h2>
                <hr>
            </div> -->
            <ol class="step-indicator">
                <li :class="this.indicatorclass(1)">
                    <div class="step"><i class="fas fa-check"></i></div>
                    <div class="caption hidden-xs hidden-sm">Trip requested</div>
                </li>
                <li :class="this.indicatorclass(2)">
                    <div class="step"><i class="fas fa-car"></i></div>
                    <div class="caption hidden-xs hidden-sm">Driver found</div>
                </li>
                <li :class="this.indicatorclass(3)">
                    <div class="step"><i class="fas fa-car-side"></i></div>
                    <div class="caption hidden-xs hidden-sm">Trip started</div>
                </li>
                <li :class="this.indicatorclass(4)">
                    <div class="step"><i class="fas fa-flag-checkered"></i></div>
                    <div class="caption hidden-xs hidden-sm">You have arrived!</div>
                </li>
            </ol>
        </div>
    </section>
    <section id="features" class="features" style="padding-top:60px;">
        <div class="container">
            <b-col md="6" offset-md="3">
                <div class="device-container">
                    <div><img class="img-fluid" src="../assets/img/yellow-car.png" alt="Yellow car"></div>
                </div>
                <p class="text-muted" style="margin-top:28px;font-size:16px;">Our drivers pass rigorous background checks and are required to maintain a high standard of driving, customer satisfaction, and vehicle maintenance. You will be rolling like a rockstar in no time!</p>
            </b-col>
        </div>
    </section>
  </div>
</template>

<script>
import { createNamespacedHelpers } from 'vuex';
const { mapGetters: commonGetters } = createNamespacedHelpers('common');
import { getDrivers, getDriver } from '@/api/drivers';
import { getPassenger } from '@/api/passengers';
import { Authentication } from '@/utils/Authentication';
const {
  mapGetters: tripGetters,
  mapActions: tripActions
} = createNamespacedHelpers('trips');

const auth = new Authentication();

export default {
  name: 'Trip',
  props: ['authenticated'],
  data() {
    return {
      drivers: [],
      selectedDriver: null,
      selectedPickUpLocation: null,
      selectedDestinationLocation: null,
      driverInfo: null,
      passengerInfo: null,
      html: '<i class="fas fa-cog fa-spin fa-3x fa-fw"></i>',
      pickUpLocations: [
        {
          id: 1,
          name: 'Microsoft Corporate Office',
          latitude: 47.6423354,
          longitude: -122.1391189
        },
        {
          id: 2,
          name: 'Microsoft Conference Center',
          latitude: 47.6384841,
          longitude: -122.1449758
        },
        {
          id: 3,
          name: 'Microsoft Production Studios',
          latitude: 47.6490121,
          longitude: -122.139642
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
    };
  },
  computed: {
    ...commonGetters(['notificationSystem']),
    ...tripGetters(['trip', 'currentStep', 'contentLoading']),
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
    }
  },
  methods: {
    ...tripActions(['setTrip', 'setCurrentStep', 'createTrip']),
    createTripRequest(trip) {
      this.createTrip(trip)
        .then(response => {
          this.$toast.success(
            `Request Code: <b>${response.code}`,
            'Driver Requested Successfully',
            this.notificationSystem.options.success
          );
        })
        .catch(err => {
          this.$toast.error(
            err.response ? err.response : err.message ? err.message : err,
            'Error',
            this.notificationSystem.options.error
          );
        });
    },
    requestDriver() {
      var user = auth.getUser();

      getPassenger(user.idToken.oid)
        .then(response => {
          this.passengerInfo = response.data;

          var trip = {
            passenger: {
              code: this.passengerInfo.email,
              firstName: this.passengerInfo.givenName,
              surname: this.passengerInfo.surname,
              //"mobileNumber": this.passengerInfo.mobileNumber,
              email: this.passengerInfo.givenName
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
        })
        .catch(err => {
          this.$toast.error(
            err.response,
            'Error',
            this.notificationSystem.options.error
          );
        });
    },
    selectPickup(location) {
      this.selectedPickUpLocation = location;
    },
    selectDestination(location) {
      this.selectedDestinationLocation = location;
    },
    indicatorclass(step) {
      return {
        active: step === this.currentstep,
        complete: this.currentstep > step
      };
    }
  }
};
</script>

<style scoped>
section.features .feature-item {
  padding-top: 0px;
  padding-bottom: 0px;
  text-align: center;
}
</style>