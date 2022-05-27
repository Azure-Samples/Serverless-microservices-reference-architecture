@description('The name of the App Service Plan that will be deployed.')
param appServicePlanName string

@description('The location that the App Service Plan will be deployed to. Default value is the location of the resource group.')
param location string = resourceGroup().location

resource plan 'Microsoft.Web/serverFarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

output appServicePlanId string = plan.id
