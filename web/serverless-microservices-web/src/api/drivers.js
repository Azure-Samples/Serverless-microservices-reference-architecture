import { checkResponse, post, get, put } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiDriversBaseUrl;
const apiKey = window.apiKey;

// GET methods
export function getDrivers() {
  return get(`${baseUrl}/drivers`, {}, apiKey).then(checkResponse);
}

export function getDriver(driverCode) {
  return get(`${baseUrl}/drivers/${driverCode}`, {}, apiKey).then(
    checkResponse
  );
}

export function getActiveDrivers() {
  return get(`${baseUrl}/activedrivers`, {}, apiKey).then(checkResponse);
}

export function getDriversWithinLocation(latitude, longitude, miles) {
  return get(
    `${baseUrl}/drivers/${latitude}/${longitude}/${miles}`,
    {},
    apiKey
  ).then(checkResponse);
}

export function getDriverLocationChanges(driverCode) {
  return get(`${baseUrl}/driverlocations/${driverCode}`, {}, apiKey).then(
    checkResponse
  );
}

// POST methods
export function createDriver(driver) {
  return post(`${baseUrl}/drivers`, driver, apiKey).then(checkResponse);
}

// PUT methods
export function updateDriver(driver) {
  return put(`${baseUrl}/drivers`, driver, apiKey).then(checkResponse);
}

export function updateDriverLocation(driver) {
  return put(`${baseUrl}/driverlocations`, driver, apiKey).then(checkResponse);
}
