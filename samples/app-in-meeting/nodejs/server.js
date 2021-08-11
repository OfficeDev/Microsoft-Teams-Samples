const express = require('express');
const bodyparser = require('body-parser');
const env = require('dotenv')
const path = require('path');
const app = express();

const server = require('http').createServer(app);
const io = require('socket.io')(server,{cors:{origin:"*"}});
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

app.get('/configure', function (req, res) {
  res.render('./views/configure');
});

app.get('/appInMeeting', function (req, res) {
    res.render('./views/appInMeeting');
});

io.sockets.on("connection", (socket) => {
  console.log(socket.id);
  socket.on("message", (message) => {
    io.emit("message", message)
  })
});

server.listen(3978, function () {
  console.log('app listening on port 3978!');
});

