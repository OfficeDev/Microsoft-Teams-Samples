const fetch = require('node-fetch');
const express = require('express');
const jwt_decode = require('jwt-decode');
const msal = require('@azure/msal-node');
const app = express();
const path = require('path');
const axios = require('axios');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
let handleQueryError = function (err) {
    console.log("handleQueryError called: ", err);
    return new Response(JSON.stringify({
        code: 400,
        message: 'Stupid network Error'
    }));
};

var name;
var photo;
var email;

app.get('/getGraphAccessToken', async (req, res) => {

    const postData = {};


    var googleAppId = process.env.GoogleAppId;
    var googleAppPassword = process.env.GoogleAppPassword;
    var redirectUrl = `${process.env.ApplicationBaseUrl}/auth-end`;
    var idToken = req.query.idToken;

    var url = `https://oauth2.googleapis.com/token?client_id=${googleAppId}&client_secret=${googleAppPassword}&code=${idToken}&redirect_uri=${redirectUrl}&grant_type=authorization_code`;
    axios.post(url, postData)
        .then(async response => {
            var url = `https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,photos,urls`;
            axios.get(url, {
                headers: {
                    "Authorization": "Bearer  " + response.data.access_token
                }
            }).then(resData => {

                name = resData.data.names[0].displayName;
                photo = resData.data.photos[0].url;
                email = resData.data.emailAddresses[0].value;
                
                var data = {
                    name: name,
                    photo: photo,
                    email: email
                }

                res.send(data).status(200);
            })
                .catch(error => {
                    console.error(error)
                })
        })
        .catch(error => {
            console.error(error)
        })
});

// Handles any requests that don't match the ones above
app.get('*', (req, res) => {
    console.log("Unhandled request: ", req);
    res.status(404).send("Path not defined");
});

const port = process.env.PORT || 5000;
app.listen(port);

console.log('API server is listening on port ' + port);