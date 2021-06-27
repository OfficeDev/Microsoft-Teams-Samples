const querystring = require('querystring');

// Get Access Token
const getAccessToken = async (req) => {
    return new Promise((resolve, reject) => {
        const { tenantId, token } = reqData(req);
        const scopes = ['User.Read', 'email', 'offline_access', 'openid', 'profile'];
        const url = `https://login.microsoftonline.com/${ tenantId }/oauth2/v2.0/token`;
        const params = {
            client_id: process.env.MicrosoftAppId,
            client_secret: process.env.MicrosoftAppPassword,
            grant_type: 'urn:ietf:params:oauth:grant-type:jwt-bearer',
            assertion: token,
            requested_token_use: 'on_behalf_of',
            scope: scopes.join(' ')
        };
        // eslint-disable-next-line no-undef
        fetch(url, {
            method: 'POST',
            body: querystring.stringify(params),
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/x-www-form-urlencoded'
            }
        }).then(result => {
            if (result.status !== 200) {
                result.json().then(json => {
                    // eslint-disable-next-line prefer-promise-reject-errors
                    reject({ error: json.error });
                });
            } else {
                result.json().then(async json => {
                    resolve(json.access_token);
                });
            }
        });
    });
};

// Parse Request
const reqData = (req) => {
    const tenantId = req.body.context.tid;
    const authHeader = req.headers.authorization;
    const token = authHeader.split(' ')[1];
    return {
        tenantId, token
    };
};

module.exports = {
    getAccessToken,
    reqData
};
