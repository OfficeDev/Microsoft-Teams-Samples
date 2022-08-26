const express = require('express');
const bodyparser = require('body-parser');
const path = require('path');
const app = express();

const server = require('http').createServer(app);
const todoData = {};
const doneData = {};
const doingData = {};

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

// parse application/json
app.use(express.json());

// Gets the meeting data based on status.
app.get('/getMeetingData', function(req, res) {
  if(req.query.status === "todo")
  {
    res.status(200).send({ data: todoData[req.query.meetingId] !== undefined ? todoData[req.query.meetingId]: undefined });;
  }
  if(req.query.status === "doing")
  {
    res.status(200).send({ data: doingData[req.query.meetingId] !== undefined ? doingData[req.query.meetingId]: undefined });;
  }
  if(req.query.status === "done")
  {
    res.status(200).send({ data: doneData[req.query.meetingId] !== undefined ? doneData[req.query.meetingId]: undefined });;
  }
});

// Saves the meeting data based on status.
app.post('/saveMeetingData', function(req, res) {
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

server.listen(3000, function () {
  console.log('app listening on port 3000!');
});