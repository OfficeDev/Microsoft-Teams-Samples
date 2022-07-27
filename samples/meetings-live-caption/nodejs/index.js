// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// index.js is used to setup and configure your tab app

// Import required packages
const path = require('path');

const express = require('express');
const cors = require('cors');
const MeetingApiHelper = require('./helpers/meetingApiHelper');
global.MeetingCartUrl = "";

// Create HTTP server.
const server = express();
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.use(express.static(path.join(__dirname, 'public')));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

server.listen(process.env.port || process.env.PORT || 3978, function () {
    console.log(`\n${server.name} listening to ${server.url}`);
});

// Returns view to be open in task module.
server.get('/Home/Index', async (req, res) => {
    res.render('./views/');
});

// Returns view to of the config page.
server.get('/Home/Configure', async (req, res) => {
    res.render('./views/configure');
});

// Method to save CART Url in the app.
server.post('/api/meeting/SaveCARTUrl', async (req, res) => {
    try
    {
        if(req.body != null && req.body.CartUrl != null){
        if (req.body.CartUrl.trim() !== "" && req.body.CartUrl.includes("meetingid") && req.body.CartUrl.includes("token"))
        {
            MeetingCartUrl = req.body.CartUrl;
            return res.status(200).send()
        }
        else
        {
            return res.status(400).send()
        }
    }
}
    catch (ex)
    {
        console.log(ex);
        return res.status(500).send();
    }
});

// Method to send caption in the live meeting.
server.post('/api/meeting/LiveCaption', async (req, res) => {
    try
    {
        var response = await MeetingApiHelper.postCaption(req.body.captionText.trim());
        return res.status(response).send();
    }
    catch (ex)
    {
        console.log(ex);
        return res.status(500).send();
    }
});