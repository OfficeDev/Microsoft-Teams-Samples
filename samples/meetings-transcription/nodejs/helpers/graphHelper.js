const axios = require('axios');
require('isomorphic-fetch');

class GraphHelper {
    constructor() {
        this._token = this.GetAccessToken();
    }

    /**
     * Gets application token.
     * @returns Application token.
     */
    GetAccessToken() {
        let qs = require('qs')
        const data = qs.stringify({
            'grant_type': 'client_credentials',
            'client_id': process.env.CLIENT_ID,
            'scope': 'https://graph.microsoft.com/.default',
            'client_secret': process.env.CLIENT_PASSWORD
        });

        return new Promise(async (resolve) => {
            const config = {
                method: 'post',
                url: 'https://login.microsoftonline.com/' + process.env.TENANT_ID + '/oauth2/v2.0/token',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                data: data
            };

            await axios(config)
                .then(function (response) {
                    resolve((response.data).access_token)
                })
                .catch(function (error) {
                    resolve(error)
                });
        })
    } 

    /**
     * Gets the meeting transcript for the passed meeting Id.
     * @param {string} meetingId Id of the meeting from Bot Framework (thread-based format)
     * @returns Transcript of meeting if any otherwise return empty string.
     */
    async GetMeetingTranscriptionsAsync(meetingId)
    {
        try
        {
            var access_Token = await this._token;
            
            // Decode the base64 meeting ID to understand the format
            let decodedId = Buffer.from(meetingId, 'base64').toString('utf-8');
            
            // Extract the thread ID (format: 19:meeting_<guid>@thread.v2)
            let threadMatch = decodedId.match(/19:meeting_[^@]+@thread\.v2/);
            
            // Try using CallRecords API to find the call and then get transcripts
            // The call records API can be queried to find recent calls
            
            if (threadMatch) {
                let threadId = threadMatch[0];
                
                try {
                    // Query call records for recent calls
                    // Note: This requires CallRecords.Read.All permission
                    var callRecordsEndpoint = `https://graph.microsoft.com/v1.0/communications/callRecords`;
                    
                    const callRecordsConfig = {
                        method: 'get',
                        url: callRecordsEndpoint,
                        headers: {
                            'Authorization': `Bearer ${access_Token}`
                        }
                    };
                    
                    var callRecordsResponse = await axios(callRecordsConfig);
                    var callRecords = callRecordsResponse.data.value;
                    
                    if (callRecords && callRecords.length > 0) {
                        // Search through call records to find the one matching our thread ID
                        let matchedCallId = null;
                        for (let record of callRecords) {
                            let callId = record.id;
                        
                        // Get full call record details including organizer
                        var callDetailsEndpoint = `https://graph.microsoft.com/v1.0/communications/callRecords/${callId}`;
                        
                        const callDetailsConfig = {
                            method: 'get',
                            url: callDetailsEndpoint,
                            headers: {
                                'Authorization': `Bearer ${access_Token}`
                            }
                        };
                        
                        var callDetails = (await axios(callDetailsConfig)).data;
                        
                        // The call record's joinWebUrl contains the meeting info
                        // Check if this joinWebUrl matches our thread ID
                        let joinUrl = callDetails.joinWebUrl;
                        
                        // Decode the URL to compare with thread ID
                        let decodedUrl = decodeURIComponent(joinUrl);
                        
                        // Check if the joinWebUrl contains our thread ID
                        if (joinUrl && decodedUrl.includes(threadId.replace('@thread.v2', ''))) {
                            matchedCallId = callId;
                            
                            if (callDetails.organizer?.user?.id) {
                                let organizerId = callDetails.organizer.user.id;
                                
                                // Try to get the organizer's meetings
                                // Note: $top is not supported with app-only permissions
                                // Try using $filter with joinWebUrl instead
                                let encodedJoinUrl = encodeURIComponent(joinUrl);
                                var organizerMeetingsEndpoint = `https://graph.microsoft.com/beta/users/${organizerId}/onlineMeetings?$filter=JoinWebUrl eq '${encodedJoinUrl}'`;
                                
                                const organizerMeetingsConfig = {
                                    method: 'get',
                                    url: organizerMeetingsEndpoint,
                                    headers: {
                                        'Authorization': `Bearer ${access_Token}`
                                    }
                                };
                                
                                try {
                                    var meetingsResponse = (await axios(organizerMeetingsConfig)).data;
                                    var meetings = meetingsResponse.value;
                                    
                                    if (meetings && meetings.length > 0) {
                                        // Match by joinWebUrl
                                        let matchedMeeting = meetings.find(m => m.joinWebUrl === joinUrl);
                                        
                                        if (matchedMeeting) {
                                            // Now get transcripts for this meeting
                                            var transcriptsEndpoint = `https://graph.microsoft.com/beta/users/${organizerId}/onlineMeetings/${matchedMeeting.id}/transcripts`;
                                            
                                            const transcriptsConfig = {
                                                method: 'get',
                                                url: transcriptsEndpoint,
                                                headers: {
                                                    'Authorization': `Bearer ${access_Token}`
                                                }
                                            };
                                            
                                            var transcriptsResponse = (await axios(transcriptsConfig)).data;
                                            var transcripts = transcriptsResponse.value;
                                            
                                            if (transcripts && transcripts.length > 0) {
                                                // Sort by createdDateTime to get the most recent transcript
                                                transcripts.sort((a, b) => new Date(b.createdDateTime) - new Date(a.createdDateTime));
                                                let latestTranscript = transcripts[0];
                                                
                                                // Get transcript content
                                                var contentEndpoint = `${transcriptsEndpoint}/${latestTranscript.id}/content?$format=text/vtt`;
                                                
                                                const contentConfig = {
                                                    method: 'get',
                                                    url: contentEndpoint,
                                                    headers: {
                                                        'Authorization': `Bearer ${access_Token}`
                                                    }
                                                };
                                                
                                                var transcript = (await axios(contentConfig)).data;
                                                return transcript;
                                            }
                                        }
                                    }
                                } catch (orgMeetingsError) {
                                    console.error("Error accessing organizer meetings:", orgMeetingsError.response?.data?.error?.message);
                                }
                            }
                            break; // Found the matching call record, exit loop
                        }
                        } // End of for loop
                    }
                    
                    return "";
                    
                } catch (callRecordsError) {
                    console.error("CallRecords API Error:", callRecordsError.response?.data?.error?.message || callRecordsError.message);
                    return "";
                }
            } else {
                return "";
            }
        }
        catch (ex)
        {
            console.error("Graph API Error:", ex.response?.data?.error?.message || ex.message);
            return "";
        }
    }

}
module.exports = GraphHelper;
