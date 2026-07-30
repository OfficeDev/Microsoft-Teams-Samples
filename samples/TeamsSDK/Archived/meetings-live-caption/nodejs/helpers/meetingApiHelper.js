const axios = require('axios');

class MeetingApiHelper {
    static cartUrl = '';

    static setCartUrl(cartUrl) {
        MeetingApiHelper.cartUrl = cartUrl;
    }

    /**
     * Post caption in live meeting.
     * @param {string} caption Caption to be post.
     * @returns Status of api operation.
     */
    static async postCaption(caption) {
        if (!MeetingApiHelper.cartUrl) {
            return 500;
        }

        const config = {
            method: 'post',
            url: MeetingApiHelper.cartUrl,
            headers: { 
                'Content-Type': 'text/plain'
            },
            data : caption
        };

        try {
            const response = await axios(config);
            return response.status;
        } catch (error) {
            console.log(error);
            return 500;
        }
    } 
}
module.exports = MeetingApiHelper;