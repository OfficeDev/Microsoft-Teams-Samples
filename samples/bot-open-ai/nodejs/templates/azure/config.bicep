@secure()
param provisionParameters object
param provisionOutputs object

// Get existing app settings for merge
var currentAppSettings = list('${ provisionOutputs.azureWebAppBotOutput.value.resourceId }/config/appsettings', '2021-02-01').properties

// Merge TeamsFx configurations to Bot resources
module teamsFxAzureWebAppBotConfig './teamsFx/azureWebAppBotConfig.bicep' = {
  name: 'teamsFxAzureWebAppBotConfig'
  params: {
    provisionParameters: provisionParameters
    provisionOutputs: provisionOutputs
    currentAppSettings: currentAppSettings
  }
}