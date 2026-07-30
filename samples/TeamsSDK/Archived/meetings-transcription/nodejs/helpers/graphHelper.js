const axios = require('axios');

class GraphHelper {
    constructor() {
        this._token = this.GetAccessToken();
    }

    /**
     * Gets application token.
     * @returns Application token.
     */
    async GetAccessToken() {
        const params = new URLSearchParams({
            grant_type: 'client_credentials',
            client_id: process.env.MicrosoftAppId,
            scope: 'https://graph.microsoft.com/.default',
            client_secret: process.env.MicrosoftAppPassword
        });

        try {
            const response = await axios.post(
                `https://login.microsoftonline.com/${process.env.MicrosoftAppTenantId}/oauth2/v2.0/token`,
                params.toString(),
                { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
            );
            return response.data.access_token;
        } catch (error) {
            console.log(error);
            return null;
        }
    }

    /**
     * Gets the meeting transcript for the passed meeting Id.
     * @param {string} meetingId Id of the meeting
     * @returns Transcript of meeting if any otherwise return empty string.
     */
    async GetMeetingTranscriptionsAsync(meetingId)
    {
        try
        {
            var access_Token = await this._token;
            var getAllTranscriptsEndpoint = `${process.env.GraphApiEndpoint}/users/${process.env.UserId}/onlineMeetings/${meetingId}/transcripts`;
            const getAllTranscriptsConfig = {
                method: 'get',
                url: getAllTranscriptsEndpoint,
                headers: {
                    'Authorization': `Bearer ${access_Token}`
                }
            }

            var transcripts = (await axios(getAllTranscriptsConfig)).data.value;

            if (transcripts != null && transcripts.length > 0)
            {
                var getTranscriptEndpoint = `${getAllTranscriptsEndpoint}/${transcripts[0].id}/content?$format=text/vtt`;
                const getTranscriptConfig = {
                    method: 'get',
                    url: getTranscriptEndpoint,
                    headers: {
                        'Authorization': `Bearer ${access_Token}`
                    }
                };

                var transcript = (await axios(getTranscriptConfig)).data;

                return transcript;
            }
            else
            {
                return "";
            }
        }
        catch (ex)
        {
            console.log(ex);
            return "";
        }
    }

}
module.exports = GraphHelper;
