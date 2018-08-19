import { checkResponse, post, get } from '@/utils/http';
//const baseUrl = 'https://ridesharedriversfunctionapp.azurewebsites.net/api';
const baseUrl = 'http://localhost:7071/api';
const code = 'code=5mh8d73e273vsa4ZeJTj5lC4dSm50JRfeyaGmECt5HDU//wFzPV93A==';

export function getDrivers() {
  return get(`${baseUrl}/drivers?${code}`).then(checkResponse);
}
