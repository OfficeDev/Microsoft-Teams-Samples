@maxLength(20)
@minLength(4)
@description('Used to generate names for all resources in this file')
param resourceBaseName string

param botAppDomain string

@maxLength(42)
param botDisplayName string

param botServiceName string = resourceBaseName
param identityName string = resourceBaseName
param botServiceSku string = 'F0'

resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: location
  name: identityName
}

// Register your web service as a bot with the Bot Framework
resource botService 'Microsoft.BotService/botServices@2021-03-01' = {
  kind: 'azurebot'
  location: 'global'
  name: botServiceName
  properties: {
    displayName: botDisplayName
    endpoint: 'https://${botAppDomain}/api/messages'

    msaAppId: identity.properties.clientId
    msaAppMSIResourceId: identity.id
    msaAppTenantId:identity.properties.tenantId
    msaAppType:'UserAssignedMSI'

    msaAppType: 'MultiTenant'
    msaAppTenantId: ''
  }
  sku: {
    name: botServiceSku
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
