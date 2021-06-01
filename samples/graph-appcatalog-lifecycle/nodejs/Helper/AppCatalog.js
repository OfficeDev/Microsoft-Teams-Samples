

const { GraphClient } = require('../Helper/graph-client');
const { restler } = require('restler');
const fs = require('fs');
const JSZip = require('jszip');
const axios = require('axios');
var gToken = '';
class AppCatalogHelper {
    async AppCatalogHelper(token) {
        gToken = token;
    }

    static async SetToken(token) {
        gToken = token;
    }

    static async GetAllapp(context) {
        if (gToken === "") {
            return null;
        }
        const client = new GraphClient(gToken);
        var data = await client.getAppList();
        return data;
    };

    static async FindApplicationByTeamsId(context) {
        if (gToken === "") {
            return null;
        }
        const client = new GraphClient(gToken);
        var data = await client.getAppByManifestId();
        return data;
    };

    static async GetappById(context) {
        if (gToken === "") {
            return null;
        }
        const client = new GraphClient(gToken);
        var data = await client.getAppById();
        return data;
    };

    static async AppStatus(context) {
        if (gToken === "") {
            return null;
        }
        const client = new GraphClient(gToken);
        var data = await client.AppStatus();
        return data;
    };

    static async ListAppHavingBot(context) {
        if (gToken === "") {
            return null;
        }
        const client = new GraphClient(gToken);
        var data = await client.ListAppHavingBot();
        return data;
    };

    static async DeleteApp() {
        if (gToken === "") {
            return null;
        }
        const client = new GraphClient(gToken);
        var data = await client.DeleteApp();
        return data;
    };

    static async ReadZipFile(headerType) {
        return new Promise(async (resolve) => {
            
            // read a zip file
            let response = fs.readFile("Manifest/manifest.zip", async (err, data) => {
                if (err) throw err;
                await JSZip.loadAsync(data).then(async (zip) => {
                    if (headerType == 'publish') {
                        response = await this.PostData(data);
                        resolve(response)
                    }
                    else {
                        response = await this.UpdateData(data);
                        resolve(response)
                    }
                });
            });
        })
    }

    static async PublishApp() {
        return new Promise(async (resolve) => {
            var data = await this.ReadZipFile("publish");
            console.log("@@response", data);
            resolve(data);
        })
    }

    static async UpdateApp() {
        return new Promise(async (resolve) => {
            var data = await this.ReadZipFile();
            resolve(data);
            console.log("@@response", data);
        });
    }

    static async PostData(data) {
        return new Promise(async (resolve) => {
            var config = {
                method: 'post',
                url: 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps',
                headers: {
                    'Authorization': gToken,
                    'Content-Type': 'application/zip'
                },
                data: data
            };

            await axios(config)
                .then(function (response) {
                    resolve("Publish app successfully")
                })
                .catch(function (error) {
                    resolve("Error in publishing App")
                });
        })
    }

    static async UpdateData(data) {
        return new Promise(async (resolve) => {
            const client = new GraphClient(gToken);
            var listApp = await client.getAppList();
            var result = listApp.value.find(item => item.displayName === "AppCatalog");
            var appId = result.id;
            var config = {
                method: 'post',
                url: 'https://graph.microsoft.com/v1.0/appCatalogs/teamsApps/' + appId + '/appDefinitions',
                headers: {
                    'Authorization': gToken,
                    'Content-Type': 'application/zip'
                },
                data: data
            };

            await axios(config)
                .then(function (response) {
                    resolve("Update app successfully");
                })
                .catch(function (error) {
                    resolve("Error in updating App");
                });
        })
    }
}
module.exports.AppCatalogHelper = AppCatalogHelper;