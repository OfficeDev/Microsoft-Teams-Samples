@secure()
param provisionParameters object
// Resources for frontend hosting
module frontendHostingProvision './provision/frontendHosting.bicep' = {
  name: 'frontendHostingProvision'
  params: {
    provisionParameters: provisionParameters
  }
}

output frontendHostingOutput object = {
  teamsFxPluginId: 'fx-resource-frontend-hosting'
  domain: frontendHostingProvision.outputs.domain
  endpoint: frontendHostingProvision.outputs.endpoint
  storageResourceId: frontendHostingProvision.outputs.resourceId
}
// Resources for identity
module userAssignedIdentityProvision './provision/identity.bicep' = {
  name: 'userAssignedIdentityProvision'
  params: {
    provisionParameters: provisionParameters
  }
}

output identityOutput object = {
  teamsFxPluginId: 'fx-resource-identity'
  identityName: userAssignedIdentityProvision.outputs.identityName
  identityResourceId: userAssignedIdentityProvision.outputs.identityResourceId
  identityClientId: userAssignedIdentityProvision.outputs.identityClientId
}
// Resources for Azure SQL
module azureSqlProvision './provision/azureSql.bicep' = {
  name: 'azureSqlProvision'
  params: {
    provisionParameters: provisionParameters
  }
}

output azureSqlOutput object = {
  teamsFxPluginId: 'fx-resource-azure-sql'
  sqlResourceId: azureSqlProvision.outputs.resourceId
  sqlEndpoint: azureSqlProvision.outputs.sqlEndpoint
  databaseName: azureSqlProvision.outputs.databaseName
}
// Resources for Azure Functions
module functionProvision './provision/function.bicep' = {
  name: 'functionProvision'
  params: {
    provisionParameters: provisionParameters
    userAssignedIdentityId: userAssignedIdentityProvision.outputs.identityResourceId
  }
}

output functionOutput object = {
  teamsFxPluginId: 'fx-resource-function'
  functionAppResourceId: functionProvision.outputs.functionAppResourceId
  functionEndpoint: functionProvision.outputs.functionEndpoint
}
