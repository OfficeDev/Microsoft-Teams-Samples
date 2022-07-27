const querystring = require('querystring');
const msal = require('@azure/msal-node');

// Get Access Token
const getAccessToken = async (req) => {
    const { tenantId, token } = reqData(req);
    const msalClient = new msal.ConfidentialClientApplication({
        auth: {
            clientId: process.env.MicrosoftAppId,
            clientSecret: process.env.MicrosoftAppPassword
        }
    });

    return new Promise((resolve, reject) => {
        const scopes = ["https://graph.microsoft.com/User.Read email offline_access openid profile"];
        msalClient.acquireTokenOnBehalfOf({
            authority: `https://login.microsoftonline.com/${tenantId}`,
            oboAssertion: token,
            scopes: scopes,
            skipCache: true
          })
          .then(result => {
            console.log(result.accessToken);
            resolve(result.accessToken);
          })
          .catch(error => {
            reject({ "error": error.errorCode });
        });
    })
    .catch((error) => {
        console.error("Failed to get auth: ", error);
    });
};

// Parse Request
const reqData = (req) => {
    const tenantId = req.body.context.user.tenant.id;
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
