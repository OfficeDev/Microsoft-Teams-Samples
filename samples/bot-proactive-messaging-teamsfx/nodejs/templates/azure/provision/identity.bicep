@secure()
param provisionParameters object
var resourceBaseName = provisionParameters.resourceBaseName
var identityName = contains(provisionParameters, 'userAssignedIdentityName') ? provisionParameters['userAssignedIdentityName'] : '${resourceBaseName}' // Try to read name for user assigned identity from parameters

// user assigned identity will be used to access other Azure resources
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: identityName
  location: resourceGroup().location
}

output identityName string = identityName
output identityClientId string = managedIdentity.properties.clientId
output identityResourceId string = managedIdentity.id
output identityPrincipalId string = managedIdentity.properties.principalId
