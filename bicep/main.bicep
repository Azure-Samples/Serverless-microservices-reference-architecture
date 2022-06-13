@description('The name of the Rideshare application.')
param applicationName string = 'Rideshare'

@allowed([
  'centralus'
  'eastus2'
  'eastasia'
  'westeurope'
  'westus2'
])
@description('The location that the Static Web App will be deployed to.')
param staticWebAppLocation string

@description('The username that will be used for the provisioned SQL Server.')
param sqlAdminLogin string

@description('The password that will be used for the provisioned SQL Server.')
@secure()
param sqlAdminPassword string

@description('Service principal Id used for deployment.')
param objectId string

@description('The resource tags that will be applied to the deployed resources.')
param resourceTags object = {
  ProjectType: 'Azure Serverless Microservices'
  Purpose: 'Sample'
}

param location string = resourceGroup().location

var functionAppServicePlanName = '${applicationName}Plan'
var keyVaultName = '${applicationName}KeyVault'
var cosmosdbName = '${applicationName}Cosmos'
var eventGridName = '${applicationName}TripExternalizations'
var signalRName = applicationName
var applicationInsightsName = '${applicationName}Insights'
var apimName = '${applicationName}Apim'
var sqlServerName = '${applicationName}-db'
var staticWebAppName = '${applicationName}Web'
var storageAccountName = take(toLower(replace('${applicationName}func', '-', '')), 24)
var functionRuntime = 'dotnet'
var functionVersion = '~4'

module keyVault 'modules/keyvault.bicep' = {
  name: keyVaultName
  params: {
    keyVaultName: keyVaultName
    objectId: objectId
    resourceTags: resourceTags
    location: location
  }
}

module cosmos 'modules/cosmosdb.bicep' = {
  name: cosmosdbName
  params: {
    accountName: cosmosdbName
    location: location
    databaseName: applicationName
    resourceTags: resourceTags
    keyVaultName: keyVault.name
  }
}

module sqlDb 'modules/sqldb.bicep' = {
  name: 'sqldb'
  params: {
    sqlServerName: sqlServerName
    sqlDatabaseName: applicationName
    administratorLogin: sqlAdminLogin
    administratorPassword: sqlAdminPassword
    location: location
    resourceTags: resourceTags
    keyVaultName: keyVault.name
  }
}

module eventGrid 'modules/eventgrid.bicep' = {
  name: eventGridName
  params: {
    eventGridTopicName: eventGridName
    location: location
    resourceTags: resourceTags
    keyVaultName: keyVault.name
  } 
}

module signalR 'modules/signalr.bicep' = {
  name: signalRName
  params: {
    signalRName: signalRName
    location: location
    resourceTags: resourceTags
    keyVaultName: keyVault.name
  } 
}

module applicationInsights 'modules/applicationInsights.bicep' = {
  name: applicationInsightsName
  params: {
    applicationInsightsName: applicationInsightsName
    location: location
    resourceTags: resourceTags
  }
}

module apim 'modules/apim.bicep' = {
  name: apimName
  params: {
    apimName: apimName
    location: location
    appInsightsName: applicationInsights.outputs.appInsightsName
    appInsightsInstrumentationKey: applicationInsights.outputs.appInsightsInstrumentationKey
    resourceTags: resourceTags
  }
}

module staticWebApp 'modules/staticwebapp.bicep' = {
  name: staticWebAppName
  params: {
    staticWebAppName: staticWebAppName
    location: staticWebAppLocation
    resourceTags: resourceTags
  }
}

module appPlan 'modules/appServicePlan.bicep' = {
  name: functionAppServicePlanName
  params: {
    appServicePlanName: functionAppServicePlanName
    location: location
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        file: {
          keyType: 'Account'
          enabled: true
        }
        blob: {
          keyType: 'Account'
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
    accessTier: 'Hot'
  }
}

resource tripFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: '${applicationName}Trips'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appPlan.outputs.appServicePlanId
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${applicationInsights.outputs.appInsightsInstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionVersion
        }
        {
          name: 'DocDbApiKey'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbPrimaryKey)'
        }
        {
          name: 'DocDbEndpointUri'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbEndpoint)'
        }
        {
          name: 'DocDbRideShareDatabaseName'
          value: cosmos.outputs.cosmosDBDatabaseName
        }
        {
          name: 'DocDbRideShareMainCollectionName'
          value: cosmos.outputs.cosmosDBRideMainCollectionName
        }
        {
          name: 'DocDbThroughput'
          value: '${cosmos.outputs.cosmosDBThroughput}'
        }
        {
          name: 'InsightsInstrumentationKey'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'IsEnqueueToOrchestrators'
          value: 'true'
        }
        {
          name: 'TripManagersQueue'
          value: 'trip-managers'
        }
        {
          name: 'TripMonitorsQueue'
          value: 'trip-monitors'
        }
        {
          name: 'TripDemosQueue'
          value: 'trip-demos'
        }
        {
          name: 'AuthorityUrl'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/AuthorityUrl)'
        }
        {
          name: 'ApiApplicationId'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/ApiApplicationId)'
        }
        {
          name: 'ApiScopeName'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/ApiScopeName)'
        }
        {
          name: 'EnableAuth'
          value: 'true'
        }
        {
          name: 'SqlConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/SqlConnectionString)'
        }
        {
          name: 'AzureSignalRConnectionString'
          value:'@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/AzureSignalRConnectionString)'
        }
      ]
      cors: {
        allowedOrigins: [
          staticWebApp.outputs.staticWebAppURL
        ]
      }
    }
    httpsOnly: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource driverFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: '${applicationName}Drivers'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appPlan.outputs.appServicePlanId
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${applicationInsights.outputs.appInsightsInstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionVersion
        }
        {
          name: 'DocDbApiKey'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbPrimaryKey)'
        }
        {
          name: 'DocDbEndpointUri'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbEndpoint)'
        }
        {
          name: 'DocDbRideShareDatabaseName'
          value: cosmos.outputs.cosmosDBDatabaseName
        }
        {
          name: 'DocDbRideShareMainCollectionName'
          value: cosmos.outputs.cosmosDBRideMainCollectionName
        }
        {
          name: 'DocDbThroughput'
          value: '${cosmos.outputs.cosmosDBThroughput}'
        }
        {
          name: 'InsightsInstrumentationKey'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'AuthorityUrl'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/AuthorityUrl)'
        }
        {
          name: 'ApiApplicationId'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/ApiApplicationId)'
        }
        {
          name: 'ApiScopeName'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/ApiScopeName)'
        }
        {
          name: 'EnableAuth'
          value: 'true'
        }
      ]
      cors: {
        allowedOrigins: [
          staticWebApp.outputs.staticWebAppURL
        ]
      }
    }
    httpsOnly: true
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource passengerFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: '${applicationName}Passengers'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appPlan.outputs.appServicePlanId
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${applicationInsights.outputs.appInsightsInstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionVersion
        }
        {
          name: 'DocDbApiKey'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbPrimaryKey)'
        }
        {
          name: 'DocDbEndpointUri'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbEndpoint)'
        }
        {
          name: 'DocDbRideShareDatabaseName'
          value: cosmos.outputs.cosmosDBDatabaseName
        }
        {
          name: 'DocDbRideShareMainCollectionName'
          value: cosmos.outputs.cosmosDBRideMainCollectionName
        }
        {
          name: 'DocDbThroughput'
          value: '${cosmos.outputs.cosmosDBThroughput}'
        }
        {
          name: 'InsightsInstrumentationKey'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'AuthorityUrl'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/AuthorityUrl)'
        }
        {
          name: 'ApiApplicationId'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/ApiApplicationId)'
        }
        {
          name: 'ApiScopeName'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/ApiScopeName)'
        }
        {
          name: 'EnableAuth'
          value: 'true'
        }
        {
          name: 'GraphTenantId'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/GraphTenantId)'
        }
        {
          name: 'GraphClientId'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/GraphClientId)'
        }
        {
          name: 'GraphClientSecret'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/GraphClientSecret)'
        }
      ]
      cors: {
        allowedOrigins: [
          staticWebApp.outputs.staticWebAppURL
        ]
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource orchestratorsFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: '${applicationName}Orchestrators'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appPlan.outputs.appServicePlanId
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value}'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: 'InstrumentationKey=${applicationInsights.outputs.appInsightsInstrumentationKey}'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: functionRuntime
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: functionVersion
        }
        {
          name: 'DocDbApiKey'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbPrimaryKey)'
        }
        {
          name: 'DocDbEndpointUri'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbEndpoint)'
        }
        {
          name: 'DocDbRideShareDatabaseName'
          value: cosmos.outputs.cosmosDBDatabaseName
        }
        {
          name: 'DocDbRideShareMainCollectionName'
          value: cosmos.outputs.cosmosDBRideMainCollectionName
        }
        {
          name: 'DocDbThroughput'
          value: '${cosmos.outputs.cosmosDBThroughput}'
        }
        {
          name: 'InsightsInstrumentationKey'
          value: applicationInsights.outputs.appInsightsInstrumentationKey
        }
        {
          name: 'DriversAcknowledgeMaxWaitPeriodInSeconds'
          value: '120'
        }
        {
          name: 'DriversLocationRadiusInMiles'
          value: '15'
        }
        {
          name: 'TripMonitorIntervalInSeconds'
          value: '10'
        }
        {
          name: 'TripMonitorMaxIterations'
          value: '20'
        }
        {
          name: 'IsPersistDirectly'
          value: 'true'
        }
        {
          name: 'TripManagersQueue'
          value: 'trip-managers'
        }
        {
          name: 'TripMonitorsQueue'
          value: 'trip-monitors'
        }
        {
          name: 'TripDemosQueue'
          value: 'trip-demos'
        }
        {
          name: 'TripDriversQueue'
          value: 'trip-drivers'
        }
        {
          name: 'TripExternalizationsEventGridTopicUrl'
          value: eventGrid.outputs.eventGripEndpoint
        }
        {
          name: 'TripExternalizationsEventGridTopicApiKey'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/TripExternalizationsEventGridTopicApiKey)'
        }
      ]
      cors: {
        allowedOrigins: [
          staticWebApp.outputs.staticWebAppURL
        ]
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource tripArchiverFunctionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: '${applicationName}TripArchiver'
  location: location
  kind: 'functionapp'
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~1'
        }
        {
          name: 'DocDbConnectionString'
          value: '@Microsoft.KeyVault(SecretUri=https:://${keyVault.name}.vault.azure.net/secrets/CosmosDbConnectionString)'
        }
      ]
      cors: {
        allowedOrigins: [
          staticWebApp.outputs.staticWebAppURL
        ]
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

module keyVaultPolicies 'modules/keyvaultPolicies.bicep' = {
  name: '${keyVaultName}polices'
  params: {
    keyVaultName: keyVaultName
    functionAppPrincipalIds: [
      tripFunctionApp.identity.principalId
      driverFunctionApp.identity.principalId
      passengerFunctionApp.identity.principalId
      tripArchiverFunctionApp.identity.principalId
      orchestratorsFunctionApp.identity.principalId
    ]
  }
}

output principalId string = orchestratorsFunctionApp.identity.principalId
