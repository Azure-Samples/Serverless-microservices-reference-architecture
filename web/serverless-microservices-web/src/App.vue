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
                  <li class="nav-item" role="presentation" v-if="this.user">
                    <a href="#" class="nav-link" @click.stop="logout()">Logout</a>
                  </li>
                  <li class="nav-item" role="presentation" v-else>
                    <a href="#" class="nav-link" @click.stop="login()">Login</a>
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
import { createNamespacedHelpers } from 'vuex';
import { Authentication } from '@/utils/Authentication';
const auth = new Authentication();
const { login, logout, getUser, getAccessToken, authenticated } = auth;
const {
  mapGetters: commonGetters,
  mapActions: commonActions
} = createNamespacedHelpers('common');

export default {
  name: 'App',
  data() {
    return {
      auth,
      authenticated
    };
  },
  computed: {
    ...commonGetters(['user'])
  },
  methods: {
    ...commonActions(['setUser']),
    login() {
      auth.login().then(
        user => {
          if (user) {
            this.setUser(user);
          } else {
            this.setUser(null);
          }
        },
        () => {
          this.setUser(null);
        }
      );
    },
    logout() {
      auth.logout().then(() => {
        this.setUser(null);
      });
    }
  },
  mounted() {
    let user = auth.getUser();
    if (user) {
      this.setUser(user);
    } else {
      this.setUser(null);
    }
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
  content: '';
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
  border-color: #4183d7;
  color: #4183d7;
}
.step-indicator li.active .caption {
  color: #4183d7;
}
.step-indicator li.complete:after {
  background-color: #87d37c;
}
.step-indicator li.complete .step {
  border-color: #87d37c;
  color: #87d37c;
}
.step-indicator li.complete .caption {
  color: #87d37c;
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
