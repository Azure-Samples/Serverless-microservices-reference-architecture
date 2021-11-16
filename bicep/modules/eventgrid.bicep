param eventGridTopicName string
param location string
param resourceTags object

resource eventGrid 'Microsoft.EventGrid/topics@2020-06-01' = {
  name: eventGridTopicName
  location: location
  tags: resourceTags
  properties:{
    publicNetworkAccess: 'Enabled'
    inputSchema: 'EventGridSchema'
  }
}

output eventGripEndpoint string = eventGrid.properties.endpoint
