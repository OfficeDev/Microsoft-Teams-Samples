// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

const auth = require("../auth");
const axios = require('axios');

class GraphHelper {

    /**
     * Creates a new meeting by provided details.
     * @param {string} userid Id of the user.
     * @param {object} excelData Details of the Excelfile to be created.
     */
    static async createMeetingAsync(userId, excelData) {
        var applicationToken = await auth.getAccessToken();

        await axios.post(`https://graph.microsoft.com/beta/users/${userId}/events`, excelData, {
            headers: {
                "authorization": "Bearer " + applicationToken,
                "contentType": 'application/json',
                "Prefer": "outlook.timezone=\"Asia/Kolkata\""
            }
        });
    }

    /**
     * Gets list of meeting present for the specified UserId.
     * @param {string} userId Id of the User.
     * @returns List of meeting present for the specified User Id.
     */
    static async listMeetingAsync(userId) {
        var applicationToken = await auth.getAccessToken();

        return await axios.get(`https://graph.microsoft.com/beta/users/${userId}/events`, {
            headers: {
                "accept": "application/json",
                "contentType": 'application/json',
                "authorization": "Bearer " + applicationToken
            }
        });
    }
}

exports.GraphHelper = GraphHelper;