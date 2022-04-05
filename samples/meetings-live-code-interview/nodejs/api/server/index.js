const express = require('express');
const PORT = process.env.PORT || 3978;
const app = express();
app.use(express.json());
app.use(express.urlencoded({
  extended: true
}));
const server = require('http').createServer(app);
const io = require('socket.io')(server, { cors: { origin: "*" } });
const editorData = {};

app.get('/api/editorState', function (req, res) {
  var questionId = req.url.substring(req.url.search('=') + 1, req.url.search('&'));
  var meetingId = req.url.split('&meetingId=')[1];
  var latestState = {
    "value": null,
  }

  if (!editorData.hasOwnProperty(meetingId)) {
    editorData[meetingId] = new Array();
    editorData[meetingId].push({
      questionId: "1",
      value: ""
    },
      {
        questionId: "2",
        value: ""
      },
      {
        questionId: "3",
        value: ""
      }
    )
  }
  else {
    editorData[meetingId].find((question) => {
      if (question.questionId == questionId) {
        latestState.value = question.value;
      }
    });
  }
  res.send(latestState);
});

io.on("connection", (socket) => {
  socket.on("message", (message, questionId, meetingId) => {
    if (editorData.hasOwnProperty(meetingId)) {
      editorData[meetingId].find((question, index) => {
        if (question.questionId == questionId) {
          editorData[meetingId][index].value = message;
        }
      });
    }
    io.emit("message", message)
  })
});

server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
});