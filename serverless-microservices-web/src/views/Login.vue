<template>
    <div>
        <header class="masthead masthead-custom">
            <div class="container h-100" style="height:155px;">
                <div class="row justify-content-center h-100" style="height:120px;">
                    <div class="col-12 col-lg-7 mt-auto" style="margin:0px;margin-top:0px;margin-bottom:0px;height:80px;max-width:100%;">
                        <div class="mx-auto header-content">
                            <h1 class="mb-5">LOGIN</h1>
                        </div>
                    </div>
                </div>
            </div>
        </header>
        <section id="features" class="features" style="padding-top:60px;">
            <div class="container">
                <h1>Login</h1>
                <pre v-text="message"></pre>
                <ul>
                    <li>
                        <button @click="login()">Login</button>
                    </li>
                    <li>
                        <button @click="callApi()">Call API</button>
                    </li>
                    <li>
                        <button @click="logout()">Logout</button>
                    </li>
                    <li>
                        <button @click="retrieveDrivers()">Get Drivers</button>
                    </li>
                </ul>
            </div>
        </section>
    </div>
</template>

<script>
import { Authentication } from '@/utils/Authentication';
import { getDrivers } from '@/api/drivers';

const auth = new Authentication();
var user = auth.getUser();
var name = (user && user.name) || '';

export default {
  name: 'Login',
  props: ['authenticated'],
  data() {
    return {
      user: auth.getUser(),
      name: name,
      message: 'hello ' + name,
      user: user,
      drivers: null
    };
  },
  methods: {
    login() {
      this.message = 'logging in...';
      auth.login();
    },
    callApi() {
      this.message = 'getting token...';
      auth
        .getAccessToken()
        .then(token => {
          this.message = 'token renewed: ' + token;
        })
        .catch(err => {
          this.message = 'error renewing token: ' + err;
        });
    },
    logout() {
      this.message = 'logging out...';
      auth.logout();
    },
    retrieveDrivers() {
      getDrivers()
        .then(response => {
          this.drivers = response.data;
        })
        .catch(err => {
          // If we are here, the token is most likely expired.
          this.message = err.response;
        });
    }
  }
};
</script>

<style scoped>
header.masthead-home {
  height: 100%;
  padding-top: 65px;
}
pre {
  height: 100px;
  background-color: lightgray;
}
</style>