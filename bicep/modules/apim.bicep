@description('API Management DB account name')
param apimName string
param appInsightsName string
param appInsightsInstrumentationKey string
param resourceTags object

@allowed([
  'Consumption'
  'Developer'
  'Basic'
  'Standard'
  'Premium'
])
@description('The pricing tier of this API Management service')
param sku string = 'Developer'

@description('The instance size of this API Management service.')
@minValue(1)
param skuCount int = 1

var location = resourceGroup().location
var publisherEmail = 'email@contoso.com'
var publisherName = 'Company Name'

resource apiManagement 'Microsoft.ApiManagement/service@2021-01-01-preview' = {
  name: apimName
  location: location
  tags: resourceTags
  sku: {
    name: sku
    capacity: skuCount
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource apiManagementLogger 'Microsoft.ApiManagement/service/loggers@2021-01-01-preview' = {
  name: appInsightsName
  parent: apiManagement
  properties: {
    loggerType: 'applicationInsights'
    description: 'Logger resources to APIM'
    credentials: {
      instrumentationKey: appInsightsInstrumentationKey
    }
  }
}

resource apimInstanceDiagnostics 'Microsoft.ApiManagement/service/diagnostics@2021-01-01-preview' = {
  name: 'applicationinsights'
  parent: apiManagement
  properties: {
    loggerId: apiManagementLogger.id
    alwaysLog: 'allErrors'
    logClientIp: true
    sampling: {
      percentage: 100
      samplingType: 'fixed'
    }
  }
}

output gatewayUrl string = apiManagement.properties.gatewayUrl
output apiIPAddress string = apiManagement.properties.publicIPAddresses[0]
