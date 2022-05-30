@description('The name of the Application insights workspace that will be deployed.')
param applicationInsightsName string

@description('The location that the Application insights workspace that will be deployed to.')
param location string

@description('The resource tags that will be applied to this Application insights workspace.')
param resourceTags object

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  kind: 'other'
  name: applicationInsightsName
  location: location
  tags: resourceTags
  properties: {
    Application_Type: 'web'
  }
}

output appInsightsName string = applicationInsights.name
output appInsightsInstrumentationKey string = applicationInsights.properties.InstrumentationKey
