// <copyright file="auth.js" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

const axios = require('axios'); 
// The auth module object.
var auth = {}; 
 
const config = require('./config');

// Function to obtain an access token using the tenant ID.
auth.getAccessToken = async function (tenantId) {
    const requestParams = new URLSearchParams({
      grant_type: 'client_credentials',
      client_id: config.BOT_ID,
      client_secret: config.BOT_PASSWORD,
      scope: 'https://graph.microsoft.com/.default'
    });

    const tokenUrl = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;
    const response = await axios.post(tokenUrl, requestParams.toString(), {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded'
      }
    });

    return response.data.access_token;
  };
  
  module.exports = auth;