import { checkResponse, postApi } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiTripsBaseUrl;
const code = window.apiTripsCode;
const apiKey = window.apiKey;

// POST methods
export function createTrip (trip) {
  return postApi(`${baseUrl}/trips?${code}`, trip, apiKey).then(checkResponse);
}
