
const { Client } = require('@microsoft/microsoft-graph-client');
const { polyfills } = require('isomorphic-fetch');
class SubscriptionManagementService {
    constructor(accessToken) {
        this.accessToken = accessToken;
        this.subscriptionPath = '/subscriptions';
    }

    getGraphClient() {
        const client = Client.init({
            authProvider: (done) => {
                done(null, this.accessToken);
            }
        });
        return client;
    }

    async deleteSubscription(subscriptionId) {
        const client = this.getGraphClient();
        await client.api(`${this.subscriptionPath}/${subscriptionId}`).delete();
    }

    async createSubscription(subscriptionCreationInformation,userId) {
        try {
            let subscription = null;
            subscriptionCreationInformation.resource="communications/presences/"+userId;
            subscriptionCreationInformation.notificationUrl=process.env.notificationUrl;
            subscriptionCreationInformation.expirationDateTime = new Date(Date.now() + 3600000).toISOString();
            const client = this.getGraphClient();
            subscription = await client.api(this.subscriptionPath).get();
            if (subscription.value == null || subscription.value.length <= 0) {
                subscription = await client.api(this.subscriptionPath).post(subscriptionCreationInformation);
            }
            return subscription;
        }
        catch (e) {
            console.log("Error--" + e);
        }
    }

}
exports.SubscriptionManagementService = SubscriptionManagementService;
