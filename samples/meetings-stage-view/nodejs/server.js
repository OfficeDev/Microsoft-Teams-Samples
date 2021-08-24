const express = require('express');
const bodyparser = require('body-parser');
const path = require('path');
const app = express();

const server = require('http').createServer(app);
const io = require('socket.io')(server,{cors:{origin:"*"}});
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

app.use("/Images", express.static(path.resolve(__dirname, './Images')));

io.on("connection", (socket) => {
  socket.on("message", (message, status) => {
    io.emit("message", message, status)
  })
});

server.listen(3978, function () {
  console.log('app listening on port 3978!');
});