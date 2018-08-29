import { checkResponse, post, get, put, getApi, postApi, putApi } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiDriversBaseUrl;
const code = window.apiDriversCode;
const apiKey = window.apiKey;

// GET methods
export function getDrivers () {
  return getApi(`${baseUrl}/drivers?${code}`, {}, apiKey).then(checkResponse);
}

export function getDriver (driverCode) {
  return getApi(
    `${baseUrl}/drivers/${driverCode}?code=gq9IhFf24ywXYsiAVpr90KtEyXBfDpubeOT3RJvbIpTSda/FAPN3ug==`, {}, apiKey
  ).then(checkResponse);
}

export function getActiveDrivers () {
  return getApi(`${baseUrl}/activedrivers?${code}`, {}, apiKey).then(checkResponse);
}

export function getDriversWithinLocation (latitude, longitude, miles) {
  return getApi(
    `${baseUrl}/drivers/${latitude}/${longitude}/${miles}?${code}`, {}, apiKey
  ).then(checkResponse);
}

export function getDriverLocationChanges (driverCode) {
  return getApi(`${baseUrl}/driverlocations/${driverCode}?${code}`, {}, apiKey).then(
    checkResponse
  );
}

// POST methods
export function createDriver (driver) {
  return postApi(`${baseUrl}/drivers?${code}`, driver, apiKey).then(checkResponse);
}

// PUT methods
export function updateDriver (driver) {
  return putApi(`${baseUrl}/drivers?${code}`, driver, apiKey).then(checkResponse);
}

export function updateDriverLocation (driver) {
  return putApi(`${baseUrl}/driverlocations?${code}`, driver, apiKey).then(checkResponse);
}
