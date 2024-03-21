@secure()
param provisionParameters object

module provision './provision.bicep' = {
  name: 'provisionResources'
  params: {
    provisionParameters: provisionParameters
  }
}

output TAB_ENDPOINT string = provision.outputs.frontendHostingOutput.endpoint
output TAB_DOMAIN string = provision.outputs.frontendHostingOutput.domain
output TAB_AZURE_STORAGE_RESOURCE_ID string = provision.outputs.frontendHostingOutput.storageResourceId
output FUNCTION_ENDPOINT string = provision.outputs.functionOutput.functionEndpoint
output FUNCTION_RESOURCE_ID string = provision.outputs.functionOutput.functionAppResourceId

module teamsFxConfig './config.bicep' = {
  name: 'addTeamsFxConfigurations'
  params: {
    provisionParameters: provisionParameters
    provisionOutputs: provision
  }
}

output provisionOutput object = provision
output teamsFxConfigurationOutput object = contains(reference(resourceId('Microsoft.Resources/deployments', teamsFxConfig.name), '2020-06-01'), 'outputs') ? teamsFxConfig : {}
