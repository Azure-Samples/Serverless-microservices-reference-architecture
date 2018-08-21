<template>
  <div>
    <header class="masthead masthead-custom">
        <div class="container h-100" style="height:155px;">
            <div class="row justify-content-center h-100" style="height:120px;">
                <div class="col-12 col-lg-7 mt-auto" style="margin:0px;margin-top:0px;margin-bottom:0px;height:80px;max-width:100%;">
                    <div class="mx-auto header-content">
                        <h1 class="mb-5">PASSENGERS<img class="float-right" src="../assets/img/rider-icon.png" alt="Passengers"></h1>
                    </div>
                </div>
            </div>
        </div>
    </header>
    <BlockUI message="Please wait..." :html="html" v-show="contentLoading"></BlockUI>
    <section id="features" class="features" style="padding-top:60px;">
        <div class="container">
            <div class="section-heading text-center" style="margin-bottom:50px;">
                <h2>View passenger information</h2>
                <p class="text-muted">View all passengers and passenger profile information</p>
                <hr>
                <div class="card border-danger">
                    <div class="card-body">
                        <h4 class="card-title">All Passengers</h4>
                        <b-table show-empty
                            responsive
                            striped
                            hover
                            :items="passengers"
                            :fields="fields"
                            :current-page="currentPage"
                            :per-page="perPage"
                            >
                            <template slot="givenName" slot-scope="row">{{row.value}}</template>
                            <template slot="surame" slot-scope="row">{{row.value}}</template>
                            <template slot="email" slot-scope="row">{{row.value}}</template>
                            <template slot="state" slot-scope="row">{{row.value}}</template>
                            <template slot="postalCode" slot-scope="row">{{row.value}}</template>
                            <template slot="actions" slot-scope="row">
                                <!-- We use @click.stop here to prevent a 'row-clicked' event from also happening -->
                                <b-button size="sm" @click.stop="selectPassenger(row.item)" class="mr-1">
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
import { getPassengers, getPassenger } from '@/api/passengers';

export default {
  name: 'Passengers',
  props: ['authenticated'],
  data() {
    return {
      message: '',
      passengers: [],
      selectedPassenger: null,
      passengerInfo: null,
      html: '<i class="fas fa-cog fa-spin fa-3x fa-fw"></i>',
      fields: [
        { key: 'givenName', label: 'First Name', sortable: true },
        { key: 'surname', label: 'Last Name', sortable: true },
        { key: 'email', label: 'Email' },
        { key: 'state', label: 'State' },
        {
          key: 'postalCode',
          label: 'PostalCode',
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
    totalRows() {
      return this.passengers.length;
    }
  },
  methods: {
    retrievePassengers() {
      getPassengers()
        .then(response => {
          this.passengers = response.data;
        })
        .catch(err => {
          // If we are here, the token is most likely expired.
          this.message = err.response;
        });
    },
    selectPassenger(passenger) {
      this.selectedPassenger = passenger;
    }
  },
  mounted() {
    this.retrievePassengers();
  }
};
</script>