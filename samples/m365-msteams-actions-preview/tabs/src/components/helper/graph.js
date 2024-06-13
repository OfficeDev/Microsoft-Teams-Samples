
export default class MicrosoftGraph {
    constructor(graphClient, objectId, itemId) {
        this.graphClient = graphClient;
        this.objectId = objectId;
        this.itemId = itemId;
    }

    /**
     * @returns the action information based itemId
     */
    async readActionItem() {
        try {
            return await this.graphClient.api(`/users/${this.objectId}/drive/items/${this.itemId}`).get();
        } catch (error) {
            console.log("readActionItem", error);
        }
    }

    /**
     * @returns the displayName of user
     */
    async getUserDisplayName() {
        try {
            return (await this.graphClient.api(`/users/${this.objectId}`).get()).displayName;
        } catch (error) {
            console.log("getUserDisplayName", error);
        }
    }
    /**
     * @returns the photo of user
     */
    async getUserPhoto() {
        try {
            return await graphClient.api(`/users/${this.objectId}/photo/$value`).get();
        } catch (error) {
            console.log("getUserPhoto", error);
        }
    }
}