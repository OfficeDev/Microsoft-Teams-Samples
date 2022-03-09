@secure()
param provisionParameters object
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
// Resources for bot
module botProvision './provision/bot.bicep' = {
  name: 'botProvision'
  params: {
    provisionParameters: provisionParameters
    userAssignedIdentityId: userAssignedIdentityProvision.outputs.identityResourceId
  }
}

output botOutput object = {
  teamsFxPluginId: 'fx-resource-bot'
  skuName: botProvision.outputs.botWebAppSKU
  siteName: botProvision.outputs.botWebAppName
  validDomain: botProvision.outputs.botDomain
  appServicePlanName: botProvision.outputs.appServicePlanName
  botWebAppResourceId: botProvision.outputs.botWebAppResourceId
  siteEndpoint: botProvision.outputs.botWebAppEndpoint
}