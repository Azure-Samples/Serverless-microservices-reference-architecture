param staticWebAppName string
param location string
param resourceTags object

resource staticWebApp 'Microsoft.Web/staticSites@2021-02-01' = {
  name: staticWebAppName
  location: location
  tags: resourceTags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    buildProperties: {
      appLocation: '/web/serverless-microservices-web'
      apiLocation: ''
      outputLocation: 'dist'
      appArtifactLocation: 'dist'
    }
  }
}

output staticWebAppURL string = staticWebApp.properties.contentDistributionEndpoint
