const express = require('express');
const bodyparser = require('body-parser');
const path = require('path');
const app = express();

const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });
const todoData = {};
const doneData = {};
const doingData = {};


app.use(express.static(__dirname + '/static'));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

app.get('/configure', function (req, res) {
  res.render('./views/configure');
});

app.get('/index', function (req, res) {
  res.render('./views/index');
});

app.get('/appInMeeting', function (req, res) {
  res.render('./views/appInMeeting');
});

app.get('/todoView', function (req, res) {
  res.render('./views/todo');
});

app.get('/doingView', function (req, res) {
  res.render('./views/doing');
});

app.get('/doneView', function (req, res) {
  res.render('./views/done');
});

app.get('/getMeetingData', function(req, res) {
  if(req.query.status === "todo")
  {
    res.status(200).send({ data: todoData[req.query.meetingId] !== undefined ? todoData[req.query.meetingId]: "" });;
  }
  if(req.query.status === "doing")
  {
    res.status(200).send({ data: doingData[req.query.meetingId] !== undefined ? doingData[req.query.meetingId]: "" });;
  }
  if(req.query.status === "done")
  {
    res.status(200).send({ data: doneData[req.query.meetingId] !== undefined ? doneData[req.query.meetingId]: "" });;
  }
});

app.use("/Images", express.static(path.resolve(__dirname, './Images')));

io.on("connection", (socket) => {
  socket.on("message", (message, status, meetingId) => {
    io.emit("message", message, status);
    if (status === "todo") {
      if (!todoData.hasOwnProperty(meetingId)) {
        todoData[meetingId] = new Array();
      }
      todoData[meetingId].push({ taskDescription: message.taskDescription, userName: message.userName });
    }
    if (status === "doing") {
      if (!doingData.hasOwnProperty(meetingId)) {
        doingData[meetingId] = new Array();
      }
      doingData[meetingId].push({ taskDescription: message.taskDescription, userName: message.userName });
    }
    if (status === "done") {
      if (!doneData.hasOwnProperty(meetingId)) {
        doneData[meetingId] = new Array();
      }
      doneData[meetingId].push({ taskDescription: message.taskDescription, userName: message.userName });
    }
  })
});

server.listen(3978, function () {
  console.log('app listening on port 3978!');
});