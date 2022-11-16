// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const auth = require("../auth");
const axios = require('axios');

class GraphHelper {

    /**
     * Creates a new tag by provided details.
     * @param {string} teamId Id of the team.
     * @param {object} teamTag Details of the tag to be created.
     */
    static async createTeamworkTagAsync(teamId, teamTag) {
        var applicationToken = await auth.getAccessToken();
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

        await axios.post(`https://graph.microsoft.com/beta/teams/${teamId}/tags`, postData, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "bearer " + applicationToken
            }
        });
    }

    /**
     * Gets list of tags present for the specified team Id.
     * @param {string} teamId Id of the team.
     * @returns List of tags present for the specified team Id.
     */
    static async listTeamworkTagsAsync(teamId) {
        var applicationToken = await auth.getAccessToken();

        return await axios.get(`https://graph.microsoft.com/beta/teams/${teamId}/tags`, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "Bearer " + applicationToken
            }
        });
    }

    /**
     * Updates an exisiting team tag.
     * @param {string} teamId Id of the team.
     * @param {object} teamTag Details of the tag to be updated.
     */
    static async updateTeamworkTagAsync(teamId, teamTag) {
        var applicationToken = await auth.getAccessToken();

        var updateTagData = {
            displayName: teamTag.displayName,
            description: teamTag.description
        }

        var response = await axios.patch(`https://graph.microsoft.com/beta/teams/${teamId}/tags/${teamTag.id}`, updateTagData, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "Bearer " + applicationToken
            }
        });

        if (response.status === 200) {
            await this.addRemoveTagMembersAsync(teamId, teamTag);
        }
    }

    /**
     * Add and remove the users as per the updated details.
     * @param {string} teamId Id of the team
     * @param {object} teamTag Updated details of the tag.
     */
    static async addRemoveTagMembersAsync(teamId, teamTag) {
        var applicationToken = await auth.getAccessToken();

        for (var member of teamTag.membersToBeAdded) {
            try {
                var teamworkTagMember =
                {
                    userId: member.userId
                };

                await axios.post(`https://graph.microsoft.com/beta/teams/${teamId}/tags/${teamTag.id}/members`, teamworkTagMember, {
                    headers: {
                        "accept": "application/json",
                        "contentType": 'application/json',
                        "authorization": "Bearer " + applicationToken
                    }
                });
            }
            catch (ex) {
                console.error("Member not added with user Id: " + member.userId, ex);
                continue;
            }
        }

        for (var member of teamTag.membersToBeDeleted)
        {
            try {

                await axios.delete(`https://graph.microsoft.com/beta/teams/${teamId}/tags/${teamTag.id}/members/${member.id}`, {
                    headers: {
                        "accept": "application/json",
                        "contentType": 'application/json',
                        "authorization": "Bearer " + applicationToken
                    }
                });
            }
            catch (ex)
            {
                console.error("Member not deleted with user Id: " + member.userId, ex);
                continue;
            }
        }
    }

    /**
     * Gets the members of the specified tag Id.
     * @param {string} teamId Id of the team.
     * @param {string} tagId Id of the tag.
     * @returns List of members.
     */
    static async getTeamworkTagMembersAsync(teamId, tagId)
    {
        var applicationToken = await auth.getAccessToken();

        var response = await axios.get(`https://graph.microsoft.com/beta/teams/${teamId}/tags/${tagId}/members`, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "Bearer " + applicationToken
            }
        });

        return response.data.value;
    }

}

exports.GraphHelper = GraphHelper;