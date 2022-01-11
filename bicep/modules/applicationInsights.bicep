param applicationInsightsName string
param location string
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
