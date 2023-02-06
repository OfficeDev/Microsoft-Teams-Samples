// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
const { Client } = require('@microsoft/microsoft-graph-client');
require('isomorphic-fetch');
/**
 * This class is a wrapper for the Microsoft Graph API.
 * See: https://developer.microsoft.com/en-us/graph for more information.
 */
 var appName = "AppCatalog";
class GraphClient {
    constructor(token) {
        if (!token || !token.trim()) {
            throw new Error('SimpleGraphClient: Invalid token received.');
        }
        this._token = token;
        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        this.graphClient = Client.init({
            authProvider: (done) => {
                done(null, this._token);
            }
        });
    }
    /**
     * Collects information about the user in the bot.
     */
    async getMe() {
        return await this.graphClient
            .api('/me')
            .get().then((res) => {
                return res;
            });
    }
    async getClient() {
        return await this.graphClient
            .api('/me')
            .get().then((res) => {
                return res;
            });
    }
    async getAppList() {
        return await this.graphClient.api('/appCatalogs/teamsApps')
            .filter('distributionMethod eq \'organization\'')
            .get().then((res) => {
                return res;
            });
    }
    async getAppById() {
        let listApp = await this.getAppList();
        let result = listApp.value.find(item => item.displayName == appName);
        let appId = "id eq " + "'" + result.id + "'";
        return await this.graphClient.api('/appCatalogs/teamsApps')
            .filter(appId)
            .get().then((res) => {
                return res;
            });
    }
    async getAppByManifestId() {
        let listApp = await this.getAppList();
        let result = listApp.value.find(item => item.displayName === appName);
        let ExternalId = "externalId eq " + "'" + result.externalId + "'";
        return await this.graphClient.api('/appCatalogs/teamsApps')
            .filter(ExternalId)
            .get().then((res) => {
                return res;
            });
    }
    async AppStatus() {
        let listApp = await this.getAppList();
        let result = listApp.value.find(item => item.displayName === appName);
        let id = "id eq " + "'" + result.id + "'";
        return await this.graphClient.api('/appCatalogs/teamsApps')
            .filter(id)
            .expand('appDefinitions')
            .get().then((res) => {
                return res;
            });
    }
    async ListAppHavingBot() {
        return await this.graphClient.api('/appCatalogs/teamsApps')
            .filter('appDefinitions/any(a:a/bot ne null)')
            .expand('appDefinitions($expand=bot)')
            .get().then((res) => {
                return res;
            });
    }
    async DeleteApp() {
        let listApp = await this.getAppList();
        let result = listApp.value.find(item => item.displayName === "AppCatalog");
        let deleteId = "/appCatalogs/teamsApps/" + result.id;
        return await this.graphClient.api(deleteId)
            .delete().then((res) => {
                return res;
            });
    }
}
exports.GraphClient = GraphClient;