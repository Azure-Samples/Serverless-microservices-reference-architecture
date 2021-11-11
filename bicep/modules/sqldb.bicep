param sqlServerName string
param sqlDatabaeName string
param location string
param administratorLogin string
@secure()
param administratorPassword string
param resourceTags object

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

resource servers_rideshare_server_name_databases_Rideshare_name 'Microsoft.Sql/servers/databases@2021-05-01-preview' = {
  parent: sqlServer
  name: sqlDatabaeName
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
