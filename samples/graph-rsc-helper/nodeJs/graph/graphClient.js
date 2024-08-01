var graph = require('@microsoft/microsoft-graph-client');
var authProvider = require('@microsoft/microsoft-graph-client/authProviders/azureTokenCredentials');
function buildGraphClient(credential) {
	const provider = new authProvider.TokenCredentialAuthenticationProvider(credential, {
		scopes: ["https://graph.microsoft.com/.default"],
	});
	
	// Initialize Graph client instance with authProvider
	const graphClient = graph.Client.initWithMiddleware({
		authProvider: provider,
	});

	return graphClient;
}

module.exports = buildGraphClient;