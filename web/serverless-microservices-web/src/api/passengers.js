import { checkResponse, post, get, getApi } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiPassengersBaseUrl;
const code = window.apiPassengersCode;
const apiKey = window.apiKey;

// GET methods
export function getPassengers() {
  return getApi(`${baseUrl}/passengers?${code}`, {}, apiKey).then(checkResponse);
}

export function getPassenger(userid) {
  return getApi(`${baseUrl}/passengers/${userid}`, {}, apiKey).then(checkResponse);
}
