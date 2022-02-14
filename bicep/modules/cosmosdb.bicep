  
@description('Cosmos DB account name')
param accountName string

@description('Location for the Cosmos DB account.')
param location string = resourceGroup().location

@description('The name for the Core (SQL) database')
param databaseName string
param resourceTags object

@description('Name of the Key Vault to store secrets in.')
param keyVaultName string

param throughput int = 400

var containerNames = [
  'main'
  'archiver'
]

var cosmosDbConnectionStringSecretName = 'CosmosDbConnectionString'
var cosmosDbPrimaryKeySecretName = 'CosmosDbPrimaryKey'

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2021-06-15' = {
  name: toLower(accountName)
  kind: 'GlobalDocumentDB'
  location: location
  tags: resourceTags
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    enableFreeTier: false
    locations: [
      {
        locationName: location
      }
    ]
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
          backupIntervalInMinutes: 240
          backupRetentionIntervalInHours: 8
      }
  }
  }
}

resource cosmosDB 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-06-15' = {
  name: '${toLower(databaseName)}'
  parent: cosmosAccount
  tags: resourceTags
  properties: {
    resource: {
      id: '${toLower(databaseName)}'
    }
    options: {
      throughput: throughput
    }
  }
}

resource containers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-06-15' = [for cotainerName in containerNames :{
  name: cotainerName
  parent: cosmosDB
  tags: resourceTags
  properties: {
    resource: {
      id: cotainerName
      partitionKey: {
        paths: [
          '/code'
        ]
      }
    }
  }
}]

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' existing = {
  name: keyVaultName
}

resource cosmosKeySecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: cosmosDbPrimaryKeySecretName
  parent: keyVault
  properties: {
    value: cosmosAccount.listKeys().primaryMasterKey
  }
}

resource cosmosConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: cosmosDbConnectionStringSecretName
  parent: keyVault
  properties: {
    value: cosmosAccount.listConnectionStrings().connectionStrings[0].connectionString
  }
}

output cosmosDBAccountName string = cosmosAccount.name
