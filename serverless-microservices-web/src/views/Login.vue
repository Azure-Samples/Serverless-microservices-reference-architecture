<template>
    <section id="features" class="features" style="padding-top:60px;">
        <div>
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
            </ul>
        </div>
    </section>
</template>

<script>
    import {Authentication} from '../utils/Authentication';

    var auth = new Authentication('c4313db3-5491-420f-9b60-2c99edf6a94c',
        'https://login.microsoftonline.com/tfp/quuxb2c.onmicrosoft.com/b2c_1_foo/v2.0', 
        ["https://quuxb2c.onmicrosoft.com/api/rideshare"]
    );
    var user = auth.getUser();
    var name = user && user.name || "";

    export default {
        name:'Login',
        data(){
            return {
                message: "hello " + name,
                user : user
            };
        },
        methods:{
            login(){
                this.message = "logging in...";
                auth.login();
            },
            callApi(){
                this.message = "getting token...";
                auth.getAccessToken()
                    .then(token => {
                        this.message = "token renewed: " + token;
                    })
                    .catch(err=>{
                        this.message = "error renewing token: " + err;
                    });
            },
            logout(){
                this.message = "logging out...";
                auth.logout();
            },
        }
    };
</script>

<style scoped>
header.masthead-home {
  height: 100%;
  padding-top: 65px;
}
pre{
    height: 100px;
    background-color: lightgray;
}
</style>