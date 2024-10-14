@maxLength(20)
@minLength(4)
@description('Used to generate names for all resources in this file')
param resourceBaseName string

@maxLength(42)
param botDisplayName string

param botServiceName string = resourceBaseName
param botServiceSku string = 'F0'
param botAadAppClientId string
param botAppDomain string

param aadAppClientId string
@secure()
param aadAppClientSecret string
param microsoftAppType string
param microsoftAppTenantId string

// Register your web service as a bot with the Bot Framework
resource botService 'Microsoft.BotService/botServices@2021-03-01' = {
  kind: 'azurebot'
  location: 'global'
  name: botServiceName
  properties: {
    displayName: botDisplayName
    endpoint: 'https://${botAppDomain}/api/messages'
    msaAppId: botAadAppClientId
    msaAppType: microsoftAppType
    msaAppTenantId: microsoftAppType == 'SingleTenant' ? microsoftAppTenantId : ''
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

resource botServiceConnection 'Microsoft.BotService/botServices/connections@2021-03-01' = {
  parent: botService
  name: 'oauthbotsetting'
  location: 'global'
  properties: {
    serviceProviderDisplayName: 'Azure Active Directory v2'
    serviceProviderId: '30dd229c-58e3-4a48-bdfd-91ec48eb906c'
    scopes: 'User.Read'
    parameters: [
      {
        key: 'clientId'
        value: aadAppClientId
      }
      {
        key: 'clientSecret'
        value: aadAppClientSecret
      }
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
