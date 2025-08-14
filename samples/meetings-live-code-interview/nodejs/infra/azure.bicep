@maxLength(20)
@minLength(4)
param resourceBaseName string
param staticWebAppSku string

param staticWebAppName string = resourceBaseName

// Azure Static Web Apps that hosts your static web site
resource swa 'Microsoft.Web/staticSites@2022-09-01' = {
  name: staticWebAppName
  // SWA do not need location setting
  location: 'centralus'
  sku: {
    name: staticWebAppSku
    tier: staticWebAppSku
  }
  properties: {}
}

var siteDomain = swa.properties.defaultHostname

// The output will be persisted in .env.{envName}. Visit https://aka.ms/teamsfx-actions/arm-deploy for more details.
output AZURE_STATIC_WEB_APPS_RESOURCE_ID string = swa.id
output TAB_DOMAIN string = siteDomain
output TAB_HOSTNAME string = siteDomain
output TAB_ENDPOINT string = 'https://${siteDomain}'