@description('Name of the Event Grid Topic')
param eventGridTopicName string

@description('The location that the Event Grid Topic will be deployed to')
param location string

@description('The resource tags that will be applied to the Event Grid resource.')
param resourceTags object

@description('Name of the Key Vault to store secrets in.')
param keyVaultName string

var eventGridTopicApiKeySecretName = 'TripExternalizationsEventGridTopicApiKey'

resource eventGrid 'Microsoft.EventGrid/topics@2020-06-01' = {
  name: eventGridTopicName
  location: location
  tags: resourceTags
  properties:{
    publicNetworkAccess: 'Enabled'
    inputSchema: 'EventGridSchema'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: keyVaultName
}

resource eventGridTopicApiKey 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: eventGridTopicApiKeySecretName
  parent: keyVault
  properties: {
    value: eventGrid.listKeys().key1
  }
}

output eventGripEndpoint string = eventGrid.properties.endpoint
