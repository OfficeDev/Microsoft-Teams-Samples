@secure()
param provisionParameters object

// Merge TeamsFx configurations to Bot service
module botProvision './provision/botService.bicep' = {
  name: 'botProvision'
  params: {
    provisionParameters: provisionParameters
    botEndpoint: azureWebAppBotProvision.outputs.siteEndpoint
  }
}

// Resources web app
module azureWebAppBotProvision './provision/azureWebAppBot.bicep' = {
  name: 'azureWebAppBotProvision'
  params: {
    provisionParameters: provisionParameters
    userAssignedIdentityId: userAssignedIdentityProvision.outputs.identityResourceId
  }
}


output azureWebAppBotOutput object = {
  teamsFxPluginId: 'teams-bot'
  skuName: azureWebAppBotProvision.outputs.skuName
  siteName: azureWebAppBotProvision.outputs.siteName
  domain: azureWebAppBotProvision.outputs.domain
  appServicePlanName: azureWebAppBotProvision.outputs.appServicePlanName
  resourceId: azureWebAppBotProvision.outputs.resourceId
  siteEndpoint: azureWebAppBotProvision.outputs.siteEndpoint
}

output BotOutput object = {
  domain: azureWebAppBotProvision.outputs.domain
  endpoint: azureWebAppBotProvision.outputs.siteEndpoint
}

// Resources for identity
module userAssignedIdentityProvision './provision/identity.bicep' = {
  name: 'userAssignedIdentityProvision'
  params: {
    provisionParameters: provisionParameters
  }
}

output identityOutput object = {
  teamsFxPluginId: 'identity'
  identityName: userAssignedIdentityProvision.outputs.identityName
  identityResourceId: userAssignedIdentityProvision.outputs.identityResourceId
  identityClientId: userAssignedIdentityProvision.outputs.identityClientId
}