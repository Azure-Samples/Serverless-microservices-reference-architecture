@description('The name of the SQL Server that will be deployed.')
param sqlServerName string

@description('The name of the database that will be deployed.')
param sqlDatabaseName string

@description('The location that our server and database will be deployed to.')
param location string

@description('The admin username for the server.')
param administratorLogin string

@description('The password for the deployed server.')
@secure()
param administratorPassword string

@description('The resource tags that will be applied to our SQL resources.')
param resourceTags object

@description('Name of the Key Vault to store secrets in.')
param keyVaultName string

var sqlDbConnectionStringSecretName = 'SqlConnectionString'

resource sqlServer 'Microsoft.Sql/servers@2021-05-01-preview' = {
  name: sqlServerName
  location: location
  tags: resourceTags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    version: '12.0'
  }
  dependsOn: []
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  tags: resourceTags
  sku: {
    name: 'S0'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 268435456000
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: false
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: keyVaultName
}

resource sqlDbConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: sqlDbConnectionStringSecretName
  parent: keyVault
  properties: {
    value: 'Server=tcp:${sqlServer.name}${environment().suffixes.sqlServerHostname},1433;Initial Catalog=db${sqlDatabase.name};Persist Security Info=False;User ID=${administratorLogin};Password=${administratorPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}
