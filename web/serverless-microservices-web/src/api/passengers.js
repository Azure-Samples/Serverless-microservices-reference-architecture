import { checkResponse, get } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiPassengersBaseUrl;
const apiKey = window.apiKey;

// GET methods
export function getPassengers() {
  return get(`${baseUrl}/passengers`, {}, apiKey).then(checkResponse);
}

export function getPassenger(userid) {
  return get(`${baseUrl}/passengers/${userid}`, {}, apiKey).then(checkResponse);
}
