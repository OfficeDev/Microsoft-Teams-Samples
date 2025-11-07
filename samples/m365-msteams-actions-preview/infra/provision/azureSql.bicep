@secure()
param provisionParameters object
var resourceBaseName = provisionParameters.resourceBaseName
var sqlServerName = contains(provisionParameters, 'sqlServerName') ? provisionParameters['sqlServerName'] : '${resourceBaseName}'
var sqlDatabaseName = contains(provisionParameters, 'sqlDatabaseName') ? provisionParameters['sqlDatabaseName'] : '${resourceBaseName}'
var sqlDatabaseSku = contains(provisionParameters, 'sqlDatabaseSku') ? provisionParameters['sqlDatabaseSku'] : 'Basic'
var administratorLogin = contains(provisionParameters, 'azureSqlAdmin') ? provisionParameters['azureSqlAdmin'] : ''
var administratorLoginPassword = contains(provisionParameters, 'azureSqlAdminPassword') ? provisionParameters['azureSqlAdminPassword'] : ''

resource sqlServer 'Microsoft.Sql/servers@2021-05-01-preview' = {
  location: resourceGroup().location
  name: sqlServerName
  properties: {
    administratorLogin: empty(administratorLogin) ? null : administratorLogin
    administratorLoginPassword: administratorLoginPassword
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-05-01-preview' = {
  parent: sqlServer
  location: resourceGroup().location
  name: sqlDatabaseName
  sku: {
    name: sqlDatabaseSku // You can follow https://aka.ms/teamsfx-bicep-add-param-tutorial to add sqlDatabaseSku property to provisionParameters to override the default value "Basic".
  }
}

resource sqlFirewallRules 'Microsoft.Sql/servers/firewallRules@2021-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzure'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

output resourceId string = sqlServer.id
output sqlEndpoint string = sqlServer.properties.fullyQualifiedDomainName
output databaseName string = sqlDatabaseName
