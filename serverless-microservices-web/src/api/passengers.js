import { checkResponse, post, get } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiPassengersBaseUrl;
const code = window.apiPassengersCode;

// GET methods
export function getPassengers() {
  return get(`${baseUrl}/passengers?${code}`).then(checkResponse);
}

export function getPassenger(userid) {
  return get(`${baseUrl}/passengers/${userid}`).then(checkResponse);
}
