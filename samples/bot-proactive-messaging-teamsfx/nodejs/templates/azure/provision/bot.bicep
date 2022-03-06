@secure()
param provisionParameters object
param userAssignedIdentityId string

var resourceBaseName = provisionParameters.resourceBaseName
var botAadAppClientId = provisionParameters['botAadAppClientId'] // Read AAD app client id for Azure Bot Service from parameters
var botServiceName = contains(provisionParameters, 'botServiceName') ? provisionParameters['botServiceName'] : '${resourceBaseName}' // Try to read name for Azure Bot Service from parameters
var botServiceSku = contains(provisionParameters, 'botServiceSku') ? provisionParameters['botServiceSku'] : 'F0' // Try to read SKU for Azure Bot Service from parameters
var botDisplayName = contains(provisionParameters, 'botDisplayName') ? provisionParameters['botDisplayName'] : '${resourceBaseName}' // Try to read display name for Azure Bot Service from parameters
var serverfarmsName = contains(provisionParameters, 'botServerfarmsName') ? provisionParameters['botServerfarmsName'] : '${resourceBaseName}bot' // Try to read name for App Service Plan from parameters
var webAppSKU = contains(provisionParameters, 'botWebAppSKU') ? provisionParameters['botWebAppSKU'] : 'F1' // Try to read SKU for Azure Web App from parameters
var webAppName = contains(provisionParameters, 'botSitesName') ? provisionParameters['botSitesName'] : '${resourceBaseName}bot' // Try to read name for Azure Web App from parameters

// Register your web service as a bot with the Bot Framework
resource botService 'Microsoft.BotService/botServices@2021-03-01' = {
  kind: 'azurebot'
  location: 'global'
  name: botServiceName
  properties: {
    displayName: botDisplayName
    endpoint: uri('https://${webApp.properties.defaultHostName}', '/api/messages')
    msaAppId: botAadAppClientId
  }
  sku: {
    name: botServiceSku // You can follow https://aka.ms/teamsfx-bicep-add-param-tutorial to add botServiceSku property to provisionParameters to override the default value "F0".
  }
}

// Connect the bot service to Microsoft Teams
resource botServiceMsTeamsChannel 'Microsoft.BotService/botServices/channels@2021-03-01' = {
  parent: botService
  location: 'global'
  name: 'MsTeamsChannel'
  properties: {
    channelName: 'MsTeamsChannel'
  }
}

// Compute resources for your Web App
resource serverfarm 'Microsoft.Web/serverfarms@2021-02-01' = {
  kind: 'app'
  location: resourceGroup().location
  name: serverfarmsName
  sku: {
    name: webAppSKU
  }
}

// Web App that hosts your bot
resource webApp 'Microsoft.Web/sites@2021-02-01' = {
  kind: 'app'
  location: resourceGroup().location
  name: webAppName
  properties: {
    serverFarmId: serverfarm.id
    keyVaultReferenceIdentity: userAssignedIdentityId // Use given user assigned identity to access Key Vault
    siteConfig: {
      appSettings: [
        {
          name: 'SCM_DO_BUILD_DURING_DEPLOYMENT'
          value: 'true' // Execute build steps on your site during deployment
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~14' // Set NodeJS version to 14.x for your site
        }
      ]
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {} // The identity is used to access other Azure resources
    }
  }
}

output botWebAppSKU string = webAppSKU
output botWebAppName string = webAppName
output botDomain string = webApp.properties.defaultHostName
output appServicePlanName string = serverfarmsName
output botServiceName string = botServiceName
output botWebAppResourceId string = webApp.id
output botWebAppEndpoint string = 'https://${webApp.properties.defaultHostName}'
