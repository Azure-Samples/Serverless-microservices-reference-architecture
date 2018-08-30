// Auth
alert('setting' + {PATH})
window.authClientId = process.env.authClientId
window.authAuthority = `${process.env.authAuthority}`
window.authScopes = [
  `${process.env.authScopes}`
]
window.authEnabled = false

// API endpoints
window.apiBaseUrl = ''
window.apiDriversBaseUrl = `${process.env.apiDriversBaseUrl}`
window.apiTripsBaseUrl = `${process.env.apiTripsBaseUrl}`
window.apiOrchestratorsBaseUrl = `${process.env.apiOrchestratorsBaseUrl}`
window.apiPassengersBaseUrl = `${process.env.apiPassengersBaseUrl}`

// API function app codes
window.apiDriversCode = `${process.env.apiDriversCode}`
window.apiTripsCode = `${process.env.apiTripsCode}`
window.apiOrchestratorsCode = `${process.env.apiOrchestratorsCode}`
window.apiPassengersCode = `${process.env.apiPassengersCode}`
