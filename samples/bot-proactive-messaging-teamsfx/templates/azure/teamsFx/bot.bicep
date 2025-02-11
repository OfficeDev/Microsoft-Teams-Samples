// Auto generated content, please customize files under provision folder

@secure()
param provisionParameters object
param provisionOutputs object
@secure()
param currentAppSettings object

param identityName string = provisionParameters.resourceBaseName

var botWebAppName = split(provisionOutputs.botOutput.value.botWebAppResourceId, '/')[8]

// Managed Identity resource
resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  location: resourceGroup().location
  name: identityName
}

resource botWebAppSettings 'Microsoft.Web/sites/config@2021-02-01' = {
  name: '${botWebAppName}/appsettings'
  properties: union({
    INITIATE_LOGIN_ENDPOINT: uri(provisionOutputs.botOutput.value.siteEndpoint, 'auth-start.html') // The page is used to let users consent required OAuth permissions during bot SSO process
    BOT_ID: identity.properties.clientId // ID of your bot
    BOT_TENANT_ID: identity.properties.tenantId // Secret of your bot
    IDENTITY_ID: provisionOutputs.identityOutput.value.identityClientId // User assigned identity id, the identity is used to access other Azure resources
    PROVISIONOUTPUT_BOTOUTPUT_SITEENDPOINT : provisionOutputs.botOutput.value.siteEndpoint // Site endpoint of AAD application
  }, currentAppSettings)
}
