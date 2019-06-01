import axios from 'axios'
import { Authentication } from '@/utils/Authentication'
const auth = new Authentication()

const validStatuses = [200, 201, 202, 203, 204, 300, 301, 302, 303, 304]

/*
 * Returns default headers list
 * which will be used with every request.
 */
function getHeaders (token, apiKey, customHeader) {
  let defaultHeaders = ''
  let authHeaders = ''

  if (apiKey) {
    defaultHeaders = {
      'Cache-Control': 'no-cache',
      'Ocp-Apim-Trace': true,
      'Ocp-Apim-Subscription-Key': apiKey
    }
  }

  if (token) {
    authHeaders = {
      Authorization: `Bearer ${token}`
    }
    if (apiKey) {
      defaultHeaders = { ...defaultHeaders, ...authHeaders }
    } else {
      defaultHeaders = authHeaders
    }
  }

  if (customHeader && customHeader !== null) {
    defaultHeaders = { ...defaultHeaders, ...customHeader }
  }

  return defaultHeaders
}

export function checkResponse (response) {
  if (validStatuses.includes(response.status)) {
    return response
  }

  // If not authorized then reset token
  // and redirect to login page
  if (response.status === 401) {
    // localStorage.removeItem('token');
    return Promise.reject(new Error('USER_ANONYMOUS'))
  }

  let err = new Error(response.statusText)
  err.response = response

  return Promise.reject(err)
}

export function processAPIErrors (apiErrors) {
  let errors = {}

  if (apiErrors) {
    for (let key of Object.keys(apiErrors)) {
      let err = apiErrors[key]

      errors[key] = err

      if (typeof err === Object || err.hasOwnProperty('length')) {
        errors[key] = apiErrors[key][0]
      }
    }
  }

  return errors
}

export const esc = encodeURIComponent

export function qs (params) {
  return Object.keys(params)
    .map(k => esc(k) + '=' + esc(params[k]))
    .join('&')
}

/*
 * Helper for POST-ing with required headers.
 */
export function post (uri, data, apiKey) {
  return auth.getAccessToken().then(token => {
    return axios.post(uri, data, {
      headers: getHeaders(token, apiKey),
      withCredentials: false
    })
  })
}

/*
 * Helper for PUT-ing with required headers.
 */
export function put (uri, data, apiKey) {
  return auth.getAccessToken().then(token => {
    return axios.put(uri, data, {
      headers: getHeaders(token, apiKey),
      withCredentials: false
    })
  })
}

/*
 * Helper for DELETE-ing with required headers.
 */
export function remove (uri, apiKey) {
  return auth.getAccessToken().then(token => {
    return axios.delete(uri, {
      headers: getHeaders(token, apiKey),
      withCredentials: false
    })
  })
}

/*
 * Helper for GET-ing with required headers.
 */
export function get (uri, data = {}, apiKey, customHeader) {
  if (Object.keys(data).length > 0) {
    uri = `${uri}?${qs(data)}`
  }
  return auth.getAccessToken().then(token => {
    return axios.get(uri, {
      headers: getHeaders(token, apiKey, customHeader),
      withCredentials: false
    })
  })
  // return axios.get(uri, {
  //   headers: getHeaders(null),
  //   withCredentials: false
  // });
}
