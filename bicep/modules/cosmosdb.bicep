  
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
var cosmosDbEndpointSecretName = 'CosmosDbEndpoint'

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

resource containers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2021-06-15' = [for containerName in containerNames :{
  name: containerName
  parent: cosmosDB
  tags: resourceTags
  properties: {
    resource: {
      id: containerName
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

resource cosmosEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2021-11-01-preview' = {
  name: cosmosDbEndpointSecretName
  parent: keyVault
  properties: {
    value: cosmosAccount.properties.documentEndpoint
  }
}

output cosmosDBAccountName string = cosmosAccount.name
output cosmosDBDatabaseName string = cosmosDB.name
output cosmosDBRideMainCollectionName string = containerNames[0]
output cosmosDBThroughput int = throughput
