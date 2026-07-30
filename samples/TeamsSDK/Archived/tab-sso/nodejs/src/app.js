'use strict';

var config = require('config');
var express = require('express');
var app = express();
var path = require('path');

// Add the route for handling tabs
var tabs = require('./server/tabs');
tabs.setup(app);

app.use(express.static(path.join(__dirname, 'client')));
  app.set('view engine', 'pug');
  app.set('views', path.join(__dirname, 'client/views'));

// Decide which port to use
var port = process.env.PORT ||
  config.has("port") ? config.get("port") : 3978;

// Listen for incoming requests
app.listen(port, function() {
  console.log(`App started listening on port ${port}`);
});
