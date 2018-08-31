import { checkResponse, post } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiTripsBaseUrl;
const apiKey = window.apiKey;

// POST methods
export function createTrip(trip) {
  return post(`${baseUrl}/trips`, trip, apiKey).then(checkResponse);
}
