@description('The name of the Key Vault resource that will be deployed.')
param keyVaultName string

@description('The prefix for the function apps.')
param functionAppPrefix string

@description('The list of function apps that will have access to this Key Vault.')
param functionApps array

@description('The resource tags that will be applied to this Key Vault.')
param resourceTags object

@description('The location that this Key Vault will be deployed to.')
param location string

resource functions 'Microsoft.Web/sites@2021-01-15' existing = [for functionApp in functionApps :{
  name: '${functionAppPrefix}${functionApp}'
}]

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enabledForTemplateDeployment: true
    tenantId: subscription().tenantId
    accessPolicies: [for i in range(0, length(functionApps)) : {
      tenantId: functions[i].identity.tenantId
      objectId: functions[i].identity.principalId
      permissions: {
        secrets: [
          'get'
        ]
      }
    }]
  }
  tags: resourceTags
}
