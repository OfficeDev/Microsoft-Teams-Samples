// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const { Client } = require('@microsoft/microsoft-graph-client');

/**
* This class is a wrapper for the Microsoft Graph API.
* See: https://developer.microsoft.com/en-us/graph for more information.
*/
class SimpleGraphClient {
    constructor(token) {
        if (!token || !token.trim()) {
            throw new Error('SimpleGraphClient: Invalid token received.');
        }

        this._token = token;

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        this.graphClient = Client.init({
            authProvider: (done) => {
                done(null, this._token); // First parameter takes an error if you can't get an access token.
            }
        });
    }

    /**
    * Creates a new tag by provided details.
    */
     async createTeamworkTagAsync(teamId, teamTag) {

        var postData = {
            displayName: teamTag.displayName,
            description: teamTag.description,
            members: []
        };

        for (var memberToBeAdded of teamTag.membersToBeAdded) {
            postData.members.push({
                userId: memberToBeAdded.userId
            });
        }

        return await this.graphClient
            .api('/teams/'+ teamId +'/tags').version('beta')
            .post(postData).then((res) => {
                return res;
        });
    }

     /**
    * Creates a new tag by provided details.
    */
      async updateTeamworkTagAsync(teamId, teamTag) {

        var updateTagData = {
            displayName: teamTag.displayName,
            description: teamTag.description
        }

        var response = await this.graphClient
            .api('/teams/'+ teamId +'/tags/'+ teamTag.id).version('beta')
            .update(updateTagData).then(async() => {
                await this.addRemoveTagMembersAsync(teamId, teamTag);
        });
    }

     /**
     * Add and remove the users as per the updated details.
     * @param {string} teamId Id of the team
     * @param {object} teamTag Updated details of the tag.
     */
      async addRemoveTagMembersAsync(teamId, teamTag) {

        for (var member of teamTag.membersToBeAdded) {
            try {
                var teamworkTagMember =
                {
                    userId: member.userId
                };

                await this.graphClient
                .api('/teams/'+ teamId +'/tags/'+ teamTag.id +'/members').version('beta')
                .post(teamworkTagMember);
            }
            catch (ex) {
                console.error("Member not added with user Id: " + member.userId, ex);
                continue;
            }
        }

        for (var member of teamTag.membersToBeDeleted)
        {
            try {
                await this.graphClient
                .api('/teams/'+ teamId +'/tags/'+ teamTag.id +'/members/'+member.id).version('beta')
                .delete();
            }
            catch (ex)
            {
                console.error("Member not deleted with user Id: " + member.userId, ex);
                continue;
            }
        }
    }

    /**
    * Gets list of tags present for the specified team Id.
    */
    async listTeamworkTagsAsync(teamId) {
        return await this.graphClient
            .api('/teams/'+ teamId +'/tags').version('beta')
            .get().then((res) => {
                return res;
        });
    }

    /**
    * Deletes the specified tag from team based on tagId and teamId.
    */
    async deleteTeamworkTagsAsync(teamId, tagId) {
        return await this.graphClient
                .api('/teams/'+ teamId +'/tags/'+ tagId).version('beta')
                .delete().then((res) => {
                    return res;
            });
    }

    /**
    * Gets the members of the specified tag Id.
    */
    async getTeamworkTagsMembersAsync(teamId, tagId) {
        return await this.graphClient
                .api('/teams/'+ teamId +'/tags/'+ tagId+'/members').version('beta')
                .get().then((res) => {
                    return res;
            });
    }
}

exports.SimpleGraphClient = SimpleGraphClient;