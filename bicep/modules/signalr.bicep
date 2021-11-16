param signalRName string
param location string
param resourceTags object

resource signalR 'Microsoft.SignalRService/SignalR@2018-10-01' = {
  name: signalRName
  location: location
  tags: resourceTags
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    size: 'F1'
    capacity: 1
  }
  properties: {
    hostNamePrefix: null
  }
}
