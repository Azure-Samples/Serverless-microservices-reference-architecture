@description('The name of the storage account that will be deployed.')
param storageAccountName string

@description('The location that the storage account will be deployed to. Default value is the location of the resource group.')
param location string = resourceGroup().location

@description('The resource tags that will be applied to the storage account.')
param resourceTags object

resource storageAccount 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: storageAccountName
  location: location
  tags: resourceTags
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

output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output storageAccountApiVersion string = storageAccount.apiVersion
