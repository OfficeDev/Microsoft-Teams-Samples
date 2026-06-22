const { Client } = require("@microsoft/microsoft-graph-client");
const { TokenCredentialAuthenticationProvider } = require("@microsoft/microsoft-graph-client/authProviders/azureTokenCredentials");
const { ClientSecretCredential } = require("@azure/identity");

class GraphClient {
    static getGraphClient() {
        const credential = new ClientSecretCredential(process.env.MicrosoftAppTenantId, process.env.MicrosoftAppId, process.env.MicrosoftAppPassword);
        const authProvider = new TokenCredentialAuthenticationProvider(credential, {
            scopes: ["https://graph.microsoft.com/.default"]
        });

        const client = Client.initWithMiddleware({
            authProvider
        });
        return client;
    }
}

module.exports = GraphClient;