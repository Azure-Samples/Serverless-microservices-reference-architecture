@description('The name of the static web app that will be deployed.')
param staticWebAppName string

@description('The location that the static web app will be deployed to.')
param location string

@description('The resource tags that will be applied to the static web app.')
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
