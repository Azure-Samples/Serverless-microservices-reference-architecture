import { checkResponse, get, post } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiTripsBaseUrl;
const apiKey = window.apiKey;

// GET methods
export function getSignalRInfo(username) {
  let customHeader = null;
  if (username) {
    customHeader = { 'x-ms-signalr-userid': username };
  }
  return get(`${baseUrl}/signalrinfo`, {}, apiKey, customHeader).then(checkResponse);
}

// POST methods
export function createTrip(trip) {
  return post(`${baseUrl}/trips`, trip, apiKey).then(checkResponse);
}
