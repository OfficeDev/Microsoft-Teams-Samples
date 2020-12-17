'use strict';

var config = require('config');
var express = require('express');
var app = express();

// Add the route for handling tabs
var tabs = require('./tabs');
tabs.setup(app);

// Decide which port to use
var port = process.env.PORT ||
           config.has("port") ? config.get("port") : 3333;

// Listen for incoming requests
app.listen(port, function() {
    console.log(`App started listening on port ${port}`);
});
