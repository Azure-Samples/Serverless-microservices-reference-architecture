<template>
  <div>
    <header class="masthead masthead-custom">
        <div class="container h-100" style="height:155px;">
            <div class="row justify-content-center h-100" style="height:120px;">
                <div class="col-12 col-lg-7 mt-auto" style="margin:0px;margin-top:0px;margin-bottom:0px;height:80px;max-width:100%;">
                    <div class="mx-auto header-content">
                        <h1 class="mb-5">DRIVERS<img class="float-right" src="../assets/img/driver-icon.png" alt="Drivers"></h1>
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
                        <b-table show-empty
                            responsive
                            striped
                            hover
                            :items="drivers"
                            :fields="fields"
                            :current-page="currentPage"
                            :per-page="perPage"
                            >
                            <template slot="code" slot-scope="row"><strong>{{row.value}}</strong></template>
                            <template slot="firstName" slot-scope="row">{{row.value}}</template>
                            <template slot="lastName" slot-scope="row">{{row.value}}</template>
                            <template slot="latitude" slot-scope="row">{{row.value}}</template>
                            <template slot="longitude" slot-scope="row">{{row.value}}</template>
                            <template slot="isAcceptingRides" slot-scope="row">{{row.value?'Yes':'No'}}</template>
                            <template slot="actions" slot-scope="row">
                                <!-- We use @click.stop here to prevent a 'row-clicked' event from also happening -->
                                <b-button size="sm" @click.stop="selectDriver(row.item)" class="mr-1">
                                Select
                                </b-button>
                            </template>
                        </b-table>
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
const {
  mapGetters: driverGetters,
  mapActions: driverActions
} = createNamespacedHelpers('drivers');

export default {
  name: 'Drivers',
  props: ['authenticated'],
  data() {
    return {
      drivers: [],
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
    ...driverGetters(['selectedDriver', 'contentLoading']),
    totalRows() {
      return this.stories.length;
    }
  },
  methods: {
    ...driverActions(['getDrivers', 'setSelectedDriver']),
    retrieveDrivers() {
      this.getDrivers()
        .then(response => {
          this.drivers = response;
        })
        .catch(err => {
          this.$toast.error(
            err.response ? err.response : err.message ? err.message : err,
            'Error',
            this.notificationSystem.options.error
          );
        });
    },
    selectDriver(driver) {
      this.setSelectedDriver(driver);
    }
  },
  mounted() {
    this.retrieveDrivers();
  }
};
</script>