const path = require('path');
const express = require('express');
const bodyparser = require('body-parser');
var cors = require('cors');

// Read botFilePath and botFileSecret from .env file.
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// Create HTTP server.
const server = express();
const port = process.env.port || process.env.PORT || 3000;
server.listen(port, () => 
    console.log(`App listening at https://localhost:${port}`)
);

// parse application/json
server.use(cors());
server.use(express.json());

const todoData = {};
const doneData = {};
const doingData = {};

// Gets the meeting data based on status.
server.get('/getMeetingData', function (req, res) {
  if (req.query.status === "todo") {
    res.status(200).send({ data: todoData[req.query.meetingId] !== undefined ? todoData[req.query.meetingId] : undefined });;
  }
  if (req.query.status === "doing") {
    res.status(200).send({ data: doingData[req.query.meetingId] !== undefined ? doingData[req.query.meetingId] : undefined });;
  }
  if (req.query.status === "done") {
    res.status(200).send({ data: doneData[req.query.meetingId] !== undefined ? doneData[req.query.meetingId] : undefined });;
  }
});

// Saves the meeting data based on status.
server.post('/saveMeetingData', function (req, res) {
  let meetingDetails = req.body;

  if (meetingDetails.status === "todo") {
    if (!todoData.hasOwnProperty(meetingDetails.meetingId)) {
      todoData[meetingDetails.meetingId] = new Array();
    }
    todoData[meetingDetails.meetingId].push(meetingDetails);
  }

  if (meetingDetails.status === "doing") {
    if (!doingData.hasOwnProperty(meetingDetails.meetingId)) {
      doingData[meetingDetails.meetingId] = new Array();
    }
    doingData[meetingDetails.meetingId].push(meetingDetails);
  }

  if (meetingDetails.status === "done") {
    if (!doneData.hasOwnProperty(meetingDetails.meetingId)) {
      doneData[meetingDetails.meetingId] = new Array();
    }
    doneData[meetingDetails.meetingId].push(meetingDetails);
  }

  res.status(200).send();
});