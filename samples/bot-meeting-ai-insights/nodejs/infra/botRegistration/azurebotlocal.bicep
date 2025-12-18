@maxLength(20)
@minLength(4)
@description('Used to generate names for all resources in this file')
param resourceBaseName string

@maxLength(42)
param botDisplayName string

param botServiceName string = resourceBaseName
param botServiceSku string = 'F0'
param aadAppClientId string
param aadAppClientSecret string
param microsoftAppTenantId string
param botAppDomain string

// Register your web service as a bot with the Bot Framework
resource botService 'Microsoft.BotService/botServices@2021-03-01' = {
  kind: 'azurebot'
  location: 'global'
  name: botServiceName
  properties: {
    displayName: botDisplayName
    endpoint: 'https://${botAppDomain}/api/messages'
    msaAppId: aadAppClientId
    msaAppTenantId: microsoftAppTenantId
    msaAppType: 'SingleTenant'
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

// Create OAuth Connection Setting for Microsoft Graph
resource botServiceConnection 'Microsoft.BotService/botServices/connections@2021-03-01' = {
  parent: botService
  name: 'graph'
  location: 'global'
  properties: {
    serviceProviderDisplayName: 'Azure Active Directory v2'
    serviceProviderId: '30dd229c-58e3-4a48-bdfd-91ec48eb906c'
    clientId: aadAppClientId
    clientSecret: aadAppClientSecret
    scopes: 'User.Read OnlineMeetings.Read OnlineMeetingArtifact.Read.All'
    parameters: [
      {
        key: 'tenantID'
        value: microsoftAppTenantId
      }
      {
        key: 'tokenExchangeUrl'
        value: 'api://botid-${aadAppClientId}'
      }
    ]
  }
}

output CONNECTION_NAME string = botServiceConnection.name
