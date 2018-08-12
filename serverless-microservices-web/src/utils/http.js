import axios from 'axios';

const http = axios.create({
  baseURL: window.apiBaseUrl
});

export function install(Vue) {
  Vue.prototype.$http = http;
}

export default http;
