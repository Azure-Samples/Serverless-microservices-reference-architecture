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
                <div class="col-lg-4">
                    <div class="device-container">
                        <div><img class="img-fluid" src="../assets/img/yellow-car.png" alt="Yellow car"></div>
                    </div>
                    <p class="text-muted" style="margin-top:28px;font-size:16px;">Our drivers pass rigorous background checks and are required to maintain a high standard of driving, customer satisfaction, and vehicle maintenance. You will be rolling like a rockstar in no time!</p>
                </div>
                <div class="col-lg-8">
                    <div class="container-fluid">
                        <div class="row">
                            <div class="col-lg-6">
                                <div class="feature-item"><i class="icon-location-pin text-primary"></i>
                                    <h3>Confirm pickup location</h3>
                                </div>
                            </div>
                            <div class="col-lg-6 align-self-center">
                                <div class="feature-item">
                                    <!-- <div class="dropdown"><button class="btn btn-info btn-block dropdown-toggle" data-toggle="dropdown" aria-expanded="false" type="button">Pickup Location</button>
                                        <div class="dropdown-menu" role="menu"><a class="dropdown-item" role="presentation" href="#">First Item</a><a class="dropdown-item" role="presentation" href="#">Second Item</a><a class="dropdown-item" role="presentation" href="#">Third Item</a></div>
                                    </div> -->
                                    <b-dropdown id="ddown-pickup" text="Pickup Location" variant="info" class="">
                                        <b-dropdown-item-button @click.stop="selectPickup(0)">Location #1</b-dropdown-item-button>
                                        <b-dropdown-item-button @click.stop="selectPickup(1)">Location #2</b-dropdown-item-button>
                                        <b-dropdown-item-button @click.stop="selectPickup(2)">Location #3</b-dropdown-item-button>
                                    </b-dropdown>
                                </div>
                            </div>
                            <div class="col-lg-6">
                                <div class="feature-item"><i class="icon-map text-primary"></i>
                                    <h3>Select destination</h3>
                                </div>
                            </div>
                            <div class="col-lg-6 align-self-center">
                                <div class="feature-item">
                                    <!-- <div class="dropdown"><button class="btn btn-info btn-block dropdown-toggle" data-toggle="dropdown" aria-expanded="false" type="button">DESTINATION</button>
                                        <div class="dropdown-menu" role="menu"><a class="dropdown-item" role="presentation" href="#">First Item</a><a class="dropdown-item" role="presentation" href="#">Second Item</a><a class="dropdown-item" role="presentation" href="#">Third Item</a></div>
                                    </div> -->
                                    <b-dropdown id="ddown-pickup" text="Destination" variant="info" class="">
                                        <b-dropdown-item-button @click.stop="selectDestination(0)">Location #1</b-dropdown-item-button>
                                        <b-dropdown-item-button @click.stop="selectDestination(1)">Location #2</b-dropdown-item-button>
                                        <b-dropdown-item-button @click.stop="selectDestination(2)">Location #3</b-dropdown-item-button>
                                    </b-dropdown>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>
  </div>
</template>

<script>
import { createNamespacedHelpers } from 'vuex';
const { mapGetters: commonGetters } = createNamespacedHelpers('common');
import { getDrivers, getDriver } from '@/api/drivers';

export default {
  name: 'Drivers',
  props: ['authenticated'],
  data() {
    return {
      drivers: [],
      selectedDriver: null,
      driverInfo: null,
      html: '<i class="fas fa-cog fa-spin fa-3x fa-fw"></i>',
      fields: [
        { key: 'code', label: 'Code', sortable: true },
        { key: 'firstName', label: 'First Name', sortable: true },
        {
          key: 'lastName',
          label: 'Last Name',
          sortable: true
        },
        { key: 'latitude', label: 'Latitude', class: 'text-right' },
        { key: 'longitude', label: 'Longitude', class: 'text-right' },
        {
          key: 'isAcceptingRides',
          label: 'Accepting rides?',
          class: 'text-right'
        },
        { key: 'actions', label: '' }
      ],
      currentPage: 1,
      perPage: 10,
      pageOptions: [5, 10, 15]
    };
  },
  computed: {
    ...commonGetters(['notificationSystem']),
    totalRows() {
      return this.stories.length;
    }
  },
  methods: {
    retrieveDrivers() {
      getDrivers()
        .then(response => {
          this.drivers = response.data;
        })
        .catch(err => {
          this.$toast.error(
            err.response,
            'Error',
            this.notificationSystem.options.error
          );
        });
    },
    selectDriver(driver) {
      this.selectedDriver = driver;
    },
    selectPickup(number) {
      this.$toast.success(
        `Set pickup location to ${number}`,
        'Success',
        this.notificationSystem.options.success
      );
    },
    selectDestination(number) {
      this.$toast.success(
        `Set destination to ${number}`,
        'Success',
        this.notificationSystem.options.success
      );
    }
  },
  mounted() {
    this.retrieveDrivers();
  }
};
</script>

<style scoped>
</style>