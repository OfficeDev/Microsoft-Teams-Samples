const axios = require('axios');

class ProactiveAppInstallationHelper {
    async getAccessToken(tenantId) {
        const params = new URLSearchParams({
            grant_type: 'client_credentials',
            client_id: process.env.MicrosoftAppId,
            scope: 'https://graph.microsoft.com/.default',
            client_secret: process.env.MicrosoftAppPassword
        });

        const response = await axios.post(
            `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`,
            params.toString(),
            { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
        );

        return response.data.access_token;
    }

    async installAppInPersonalScope(tenantId, userId) {
        const accessToken = await this.getAccessToken(tenantId);

        const data = {
            'teamsApp@odata.bind': `https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/${process.env.AppCatalogTeamAppId}`
        };

        try {
            const response = await axios.post(
                `https://graph.microsoft.com/v1.0/users/${userId}/teamwork/installedApps`,
                data,
                {
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': `Bearer ${accessToken}`
                    }
                }
            );
            return response.status;
        } catch (error) {
            if (error.response && error.response.status === 409) {
                await this.triggerConversationUpdate(tenantId, userId, accessToken);
            }
            return error.response ? error.response.status : null;
        }
    }

    async triggerConversationUpdate(tenantId, userId, accessToken) {
        const response = await axios.get(
            `https://graph.microsoft.com/v1.0/users/${userId}/teamwork/installedApps?$expand=teamsApp,teamsAppDefinition&$filter=teamsApp/externalId eq '${process.env.MicrosoftAppId}'`,
            { headers: { 'Authorization': `Bearer ${accessToken}` } }
        );

        const installedApps = response.data.value;
        if (installedApps && installedApps.length > 0) {
            for (const app of installedApps) {
                await axios.get(
                    `https://graph.microsoft.com/v1.0/users/${userId}/teamwork/installedApps/${app.id}/chat`,
                    { headers: { 'Authorization': `Bearer ${accessToken}` } }
                );
            }
        }
    }
}

module.exports = ProactiveAppInstallationHelper;
