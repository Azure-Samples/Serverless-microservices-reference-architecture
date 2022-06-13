@description('The name of the Key Vault resource that will be deployed.')
param keyVaultName string

@description('The list of function app principal Id that will have access to this Key Vault.')
param functionAppPrincipalIds array

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: keyVaultName
}

resource policies 'Microsoft.KeyVault/vaults/accessPolicies@2021-06-01-preview' = {
  name: 'add'
  parent: keyVault
  properties: {
    accessPolicies: [for i in range(0, length(functionAppPrincipalIds)) : {
      tenantId: subscription().tenantId
      objectId: functionAppPrincipalIds[i]
      permissions: {
        secrets: [
          'get'
        ]
      }
    }]
  }
}
