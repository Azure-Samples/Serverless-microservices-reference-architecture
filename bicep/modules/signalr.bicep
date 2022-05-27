@description('The name of the SignalR service that will be deployed.')
param signalRName string

@description('The location that the SignalR service will be deployed to.')
param location string

@description('The resource tags that will be applied to the SignalR resource.')
param resourceTags object

@description('Name of the Key Vault to store secrets in.')
param keyVaultName string

var signalRConnectionStringSecretName = 'AzureSignalRConnectionString'

resource signalR 'Microsoft.SignalRService/SignalR@2018-10-01' = {
  name: signalRName
  location: location
  tags: resourceTags
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    size: 'F1'
    capacity: 1
  }
  properties: {
    hostNamePrefix: null
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: keyVaultName
}

resource signalRConnectionString 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: signalRConnectionStringSecretName
  parent: keyVault
  properties: {
    value: signalR.listKeys().primaryConnectionString
  }
}
