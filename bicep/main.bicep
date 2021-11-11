param applicationName string = 'Rideshare'
param resourceTags object = {
  ProjectType: 'Azure Serverless Microservices'
  Purpose: 'Sample'
}
param sqlAdminLogin string
@secure()
param sqlAdminPassword string

var primaryLocation = 'westus2'
var alternateLocation = 'westus'

var functionAppServicePlanName = '${applicationName}Plan'
var keyVaultName = '${applicationName}KeyVault'
var cosmosdbName = '${applicationName}Cosmos'
var eventGridName = '${applicationName}TripExternalizations'
var signalRName = applicationName
var applicationInsightsName = '${applicationName}Insights'
var apimName = '${applicationName}Apim'
var sqlServerName = '${applicationName}-db'
var staticWebAppName = '${applicationName}Web'
var storageAccountName = toLower('${applicationName}functionstore')
var functionsApps = [
  'Trips'
  'Drivers'
  'Passengers'
  'TripArchiver'
  'Orchestrators'
 ]

module cosmos 'modules/cosmosdb.bicep' = {
  name: cosmosdbName
  params: {
    accountName: cosmosdbName
    location: primaryLocation
    databaseName: applicationName
    resourceTags: resourceTags
  }
}

module sqlDb 'modules/sqldb.bicep' = {
  name: 'sqldb'
  params: {
    sqlServerName: sqlServerName
    sqlDatabaeName: applicationName
    administratorLogin: sqlAdminLogin
    administratorPassword: sqlAdminPassword
    location: primaryLocation
    resourceTags: resourceTags
  }
}

module eventGrid 'modules/eventgrid.bicep' = {
  name: eventGridName
  params: {
    eventGridTopicName: eventGridName
    location: primaryLocation
    resourceTags: resourceTags
  } 
}

module signalR 'modules/signalr.bicep' = {
  name: signalRName
  params: {
    signalRName: signalRName
    location: primaryLocation
    resourceTags: resourceTags
  } 
}

module applicationInsights 'modules/applicationInsights.bicep' = {
  name: applicationInsightsName
  params: {
    applicationInsightsName: applicationInsightsName
    location: alternateLocation
    resourceTags: resourceTags
  }
}

module apim 'modules/apim.bicep' = {
  name: apimName
  params: {
    apimName: apimName
    appInsightsName: applicationInsights.outputs.appInsightsName
    appInsightsInstrumentationKey: applicationInsights.outputs.appInsightsInstrumentationKey
    resourceTags: resourceTags
  }
}

module staticeWebApp 'modules/staticwebapp.bicep' = {
  name: staticWebAppName
  params: {
    staticWebAppName: staticWebAppName
    location: primaryLocation
    resourceTags: resourceTags
  }
}

module functions 'modules/functions.bicep' = {
  name: 'functions'
  params: {
    storageAccountName: storageAccountName
    functionAppPrefix: applicationName
    functionApps: functionsApps
    appServicePlanName: functionAppServicePlanName
    location: primaryLocation
    staticWebAppURL: staticeWebApp.outputs.staticWebAppURL
    appInsightsInstrumentationKey: applicationInsights.outputs.appInsightsInstrumentationKey
    resourceTags: resourceTags
  }
}

module keyVault 'modules/keyvault.bicep' = {
  name: keyVaultName
  params: {
    keyVaultName: keyVaultName
    functionAppPrefix: applicationName
    functionApps: functionsApps
    resourceTags: resourceTags
  }
  dependsOn: [
    functions
  ]
}

