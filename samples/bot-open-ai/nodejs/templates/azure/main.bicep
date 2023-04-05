@secure()
param provisionParameters object

module provision './provision.bicep' = {
  name: 'provisionResources'
  params: {
    provisionParameters: provisionParameters
  }
}
output provisionOutput object = provision
module config './config.bicep' = {
  name: 'configureResources'
  params: {
    provisionParameters: provisionParameters
    provisionOutputs: provision
  }
}
output configOutput object = contains(reference(resourceId('Microsoft.Resources/deployments', config.name), '2020-06-01'), 'outputs') ? config : {}
