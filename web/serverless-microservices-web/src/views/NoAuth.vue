<template>
  <div>
    <header class="masthead masthead-custom">
        <div class="container h-100" style="height:155px;">
            <div class="row justify-content-center h-100" style="height:120px;">
                <div class="col-12 col-lg-7 mt-auto" style="margin:0px;margin-top:0px;margin-bottom:0px;height:80px;max-width:100%;">
                    <div class="mx-auto header-content">
                        <h1 class="mb-5">NO ACCESS</h1>
                    </div>
                </div>
            </div>
        </div>
    </header>
    <section id="features" class="features" style="padding-top:60px;">
        <div class="container">
            <div class="section-heading text-center" style="margin-bottom:50px;">
                <h2>You're not signed in</h2>
                <p class="text-muted">&nbsp;</p>
                <p>Please <a href="javascript:void(0)" class="btn btn-danger action-button" role="button" @click="login()">log in</a> to get started!</p>
                <hr />
                <p class="text-muted">If you don't have an account, choose the <strong>sign up</strong> link below the login form to create an account.</p>
                <p class="text-muted"><strong>Please note: </strong>the login form displays in a popup window. Please disable your browser's popup blocker for this site.</p>
            </div>
        </div>
    </section>
  </div>
</template>

<script>
import { createNamespacedHelpers } from 'vuex';
import { Authentication } from '@/utils/Authentication';
const auth = new Authentication();
const { login, logout, getUser, getAccessToken, authenticated } = auth;
const { mapActions: commonActions } = createNamespacedHelpers('common');

export default {
  name: 'NoAccess',
  props: ['authenticated'],
  computed: {},
  methods: {
    ...commonActions(['setUser']),
    login() {
      auth.login().then(
        user => {
          if (user) {
            this.setUser(user);
            // let redirect = this.$route.query.redirect;
            // if (redirect) {
            //   this.$router.push(redirect);
            // } else {
            //   this.$router.push('/');
            // }
          } else {
            this.setUser(null);
            // this.$router.push('/');
          }
          this.$router.push('/');
        },
        () => {
          this.setUser(null);
          this.$router.push('/');
        }
      );
    }
  },
  mounted() {}
};
</script>