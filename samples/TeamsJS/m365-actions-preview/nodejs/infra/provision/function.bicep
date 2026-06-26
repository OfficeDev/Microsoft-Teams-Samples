@secure()
param provisionParameters object
param userAssignedIdentityId string

var resourceBaseName = provisionParameters.resourceBaseName
var serverfarmsName = contains(provisionParameters, 'functionServerfarmsName') ? provisionParameters['functionServerfarmsName'] : '${resourceBaseName}api'
var serverfarmsSku = contains(provisionParameters, 'functionServerfarmsSku') ? provisionParameters['functionServerfarmsSku'] : 'Y1'
var functionAppName = contains(provisionParameters, 'functionWebappName') ? provisionParameters['functionWebappName'] : '${resourceBaseName}api'
var storageName = contains(provisionParameters, 'functionStorageName') ? provisionParameters['functionStorageName'] : '${resourceBaseName}api'
var storageSku = contains(provisionParameters, 'functionStorageSku') ? provisionParameters['functionStorageSku'] : 'Standard_LRS'

resource serverfarms 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: serverfarmsName
  kind: 'functionapp'
  location: resourceGroup().location
  sku: {
    name: serverfarmsSku // You can follow https://aka.ms/teamsfx-bicep-add-param-tutorial to add functionServerfarmsSku property to provisionParameters to override the default value "Y1".
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  name: functionAppName
  kind: 'functionapp'
  location: resourceGroup().location
  properties: {
    serverFarmId: serverfarms.id
    keyVaultReferenceIdentity: userAssignedIdentityId
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsDashboard'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'node'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storage.name};AccountKey=${listKeys(storage.id, storage.apiVersion).keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~18'
        }
      ]
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
}

resource storage 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageName
  kind: 'StorageV2'
  location: resourceGroup().location
  sku: {
    name: storageSku // You can follow https://aka.ms/teamsfx-bicep-add-param-tutorial to add functionStorageSku property to provisionParameters to override the default value "Standard_LRS".
  }
}

output functionEndpoint string = 'https://${functionApp.properties.defaultHostName}'
output functionAppResourceId string = functionApp.id
