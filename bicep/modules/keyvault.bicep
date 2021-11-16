param keyVaultName string
param functionAppPrefix string
param functionApps array
param resourceTags object

resource functions 'Microsoft.Web/sites@2021-01-15' existing = [for functionApp in functionApps :{
  name: '${functionAppPrefix}${functionApp}'
}]

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: keyVaultName
  location: resourceGroup().location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
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

