<template>
  <div>
    <nav v-scroll="handleScroll" class="navbar navbar-light navbar-expand-lg fixed-top" id="mainNav">
      <div class="container">
        <router-link :to="{ name: 'home' }" class="navbar-brand">Rideshare by Relecloud</router-link>
        <button class="navbar-toggler float-right" data-toggle="collapse"
          data-target="#navbarResponsive" aria-controls="navbarResponsive"
          aria-expanded="false" aria-label="Toggle navigation"><i class="fa fa-bars"></i>
        </button>
          <div
              class="collapse navbar-collapse" id="navbarResponsive">
              <ul class="nav navbar-nav ml-auto">
                  <li class="nav-item" role="presentation">
                    <router-link :to="{ name: 'trip' }" class="nav-link">My Trip</router-link>
                  </li>
                  <li class="nav-item" role="presentation">
                    <router-link :to="{ name: 'passengers' }" class="nav-link">Passengers</router-link>
                  </li>
                  <li class="nav-item" role="presentation">
                    <router-link :to="{ name: 'drivers' }" class="nav-link">Drivers</router-link>
                  </li>
                  <li class="nav-item" role="presentation">
                    <router-link :to="{ name: 'login' }" class="nav-link">Login</router-link>
                  </li>
              </ul>
        </div>
      </div>
    </nav>
      <router-view
        :authenticated="authenticated"
      ></router-view>
  </div>
</template>

<script>
import { Authentication } from '@/utils/Authentication';
const auth = new Authentication();
const { login, logout, getUser, getAccessToken, authenticated } = auth;

export default {
  name: 'App',
  data() {
    return {
      auth,
      authenticated
    };
  },
  methods: {
    login,
    logout
  }
};
</script>

<style>
#mainNav .navbar-nav > li > a.router-link-exact-active {
  color: #fdcc52 !important;
  background-color: transparent;
}

header.masthead-custom {
  background: url('assets/img/bg-pattern.png'),
    linear-gradient(to left, #7b4397, #dc2430);
  height: 120px;
  min-height: 140px;
}
</style>
