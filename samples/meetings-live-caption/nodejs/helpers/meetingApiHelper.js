const axios = require('axios');
require('isomorphic-fetch');

class MeetingApiHelper {

    /**
     * Post caption in live meeting.
     * @param {string} caption Caption to be post.
     * @returns Status of api operation.
     */
    static postCaption(caption) {
        const config = {
            method: 'post',
            url: MeetingCartUrl,
            headers: { 
                'Content-Type': 'text/plain'
            },
            data : caption
        };

        return new Promise(async (resolve) => {
            await axios(config)
                .then(function (response) {
                    resolve(response.status);
                })
                .catch(function (error) {
                    console.log(error);
                    resolve(500);
                });
        });
    } 
}
module.exports = MeetingApiHelper;