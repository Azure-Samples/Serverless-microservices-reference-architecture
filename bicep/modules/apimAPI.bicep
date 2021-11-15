param apimName string
param currentResourceGroup string
param backendApiName string
param apiName string
param originUrl string

var functionAppKeyName = '${backendApiName}-key'

resource backendApiApp 'Microsoft.Web/sites@2021-01-15' existing = {
  name: backendApiName
  scope: resourceGroup(currentResourceGroup)
}

resource functionKey 'Microsoft.Web/sites/functions/keys@2021-01-15' existing = {
  name: '${backendApiName}/GetTodoItems/default'
  scope: resourceGroup(currentResourceGroup)
}

resource apim 'Microsoft.ApiManagement/service@2021-01-01-preview' existing = {
  name: apimName
}

resource namedValues 'Microsoft.ApiManagement/service/namedValues@2021-01-01-preview' = {
  parent: apim
  name: functionAppKeyName
  properties: {
    displayName: functionAppKeyName
    value: listKeys('${backendApiApp.id}/host/default','2019-08-01').functionKeys.default
  }
}

resource backendApi 'Microsoft.ApiManagement/service/backends@2021-01-01-preview' = {
  parent: apim
  name: backendApiName
  properties: {
    description: backendApiName
    resourceId: 'https://management.azure.com${backendApiApp.id}'
    credentials: {
      header:{
        'x-functions-key': [
          '{{${namedValues.properties.displayName}}}'
        ]
      }
    }
    url: 'https://${backendApiApp.properties.hostNames[0]}/api'
    protocol: 'http'
  }
}

resource api 'Microsoft.ApiManagement/service/apis@2021-01-01-preview' = {
  parent: apim
  name: apiName
  properties: {
    path: apiName
    displayName: apiName
    isCurrent: true
    subscriptionRequired: false
    protocols: [
      'https'
    ]
  }
}

resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2021-01-01-preview' = {
  parent: api
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: replace(loadTextContent('../content/cos-policy.xml'),'__ORIGIN__',originUrl)
  }
}

resource opGetTodos 'Microsoft.ApiManagement/service/apis/operations@2021-01-01-preview' = {
  name: 'getTodoList'
  parent: api
  properties: {
    displayName: 'Get Todo List'
    method: 'GET'
    urlTemplate: '/todos'
  }
}

resource opGetTodosPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-01-01-preview' = {
  parent: opGetTodos
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: replace(loadTextContent('../content/api-policy.xml'),'__BACKEND-ID__',backendApi.name)
  }
}

resource opGetTodosById 'Microsoft.ApiManagement/service/apis/operations@2021-01-01-preview' = {
  name: 'getTodoItem'
  parent: api
  properties: {
    displayName: 'Get Todo Item'
    method: 'GET'
    urlTemplate: '/todos/{id}'
    templateParameters: [
      {
        name: 'id'
        required: true
        type: 'String'
      }
    ]
  }
}

resource opGetTodosByIdPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-01-01-preview' = {
  parent: opGetTodosById
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: replace(loadTextContent('../content/api-policy.xml'),'__BACKEND-ID__',backendApi.name)
  }
}

resource opPostTodoItem 'Microsoft.ApiManagement/service/apis/operations@2021-01-01-preview' = {
  name: 'postTodoItem'
  parent: api
  properties: {
    displayName: 'Create Todo Item'
    method: 'POST'
    urlTemplate: '/todos'
  }
}

resource opPostTodoItemPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-01-01-preview' = {
  parent: opPostTodoItem
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: replace(loadTextContent('../content/api-policy.xml'),'__BACKEND-ID__',backendApi.name)
  }
}

resource opPutTodosById 'Microsoft.ApiManagement/service/apis/operations@2021-01-01-preview' = {
  name: 'putTodoItem'
  parent: api
  properties: {
    displayName: 'Update Todo Item'
    method: 'PUT'
    urlTemplate: '/todos/{id}'
    templateParameters: [
      {
        name: 'id'
        required: true
        type: 'String'
      }
    ]
  }
}

resource opPutTodosByIdPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-01-01-preview' = {
  parent: opPutTodosById
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: replace(loadTextContent('../content/api-policy.xml'),'__BACKEND-ID__',backendApi.name)
  }
}

resource opDeleteTodosById 'Microsoft.ApiManagement/service/apis/operations@2021-01-01-preview' = {
  name: 'deleteTodoItem'
  parent: api
  properties: {
    displayName: 'Delete Todo Item'
    method: 'DELETE'
    urlTemplate: '/todos/{id}'
    templateParameters: [
      {
        name: 'id'
        required: true
        type: 'String'
      }
    ]
  }
}

resource opDeleteTodosByIdPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-01-01-preview' = {
  parent: opDeleteTodosById
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: replace(loadTextContent('../content/api-policy.xml'),'__BACKEND-ID__',backendApi.name)
  }
}

resource opHealthCheck 'Microsoft.ApiManagement/service/apis/operations@2021-01-01-preview' = {
  name: 'HealthCheck'
  parent: api
  properties: {
    displayName: 'Health Probe'
    method: 'HEAD'
    urlTemplate: '/todos'
  }
}

resource opHealthCheckPolicy 'Microsoft.ApiManagement/service/apis/operations/policies@2021-01-01-preview' = {
  parent: opHealthCheck
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: replace(loadTextContent('../content/api-policy.xml'),'__BACKEND-ID__',backendApi.name)
  }
}
