import { checkResponse, post, get, put } from '@/utils/http';
//const baseUrl = 'http://localhost:7071/api';
const baseUrl = window.apiDriversBaseUrl;
const code = window.apiDriversCode;

// GET methods
export function getDrivers() {
  return get(`${baseUrl}/drivers?${code}`).then(checkResponse);
}

export function getDriver(driverCode) {
  return get(
    `${baseUrl}/drivers/${driverCode}?code=gq9IhFf24ywXYsiAVpr90KtEyXBfDpubeOT3RJvbIpTSda/FAPN3ug==`
  ).then(checkResponse);
}

export function getActiveDrivers() {
  return get(`${baseUrl}/activedrivers?${code}`).then(checkResponse);
}

export function getDriversWithinLocation(latitude, longitude, miles) {
  return get(
    `${baseUrl}/drivers/${latitude}/${longitude}/${miles}?${code}`
  ).then(checkResponse);
}

export function getDriverLocationChanges(driverCode) {
  return get(`${baseUrl}/driverlocations/${driverCode}?${code}`).then(
    checkResponse
  );
}

// POST methods
export function createDriver(driver) {
  return post(`${baseUrl}/drivers?${code}`, driver).then(checkResponse);
}

// PUT methods
export function updateDriver(driver) {
  return put(`${baseUrl}/drivers?${code}`, driver).then(checkResponse);
}

export function updateDriverLocation(driver) {
  return put(`${baseUrl}/driverlocations?${code}`, driver).then(checkResponse);
}
