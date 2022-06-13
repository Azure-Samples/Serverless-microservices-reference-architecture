@description('The name of the Key Vault resource that will be deployed.')
param keyVaultName string

@description('Service principal Id used for deployment.')
param objectId string

@description('The resource tags that will be applied to this Key Vault.')
param resourceTags object

@description('The location that this Key Vault will be deployed to.')
param location string

resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      name: 'standard'
      family: 'A'
    }
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: objectId
        permissions: {
          secrets: [
            'all'
          ]
        }
      }
    ]
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    enabledForTemplateDeployment: true
    tenantId: subscription().tenantId
  }
  tags: resourceTags
}
