const express = require('express');
const PORT = process.env.PORT || 3000;
const app = express();
app.use(express.json());
app.use(express.urlencoded({
  extended: true
}));
const server = require('http').createServer(app);
const editorData = {};

app.get('/api/editor', function (req, res) {
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

app.post('/api/Save', function (req, res) {
  var meetingId = req.body.meetingId;
  var editorValue = req.body.editorData;
  var questionId = req.body.questionId;

  if (editorData.hasOwnProperty(meetingId)) {
    editorData[meetingId].find((question, index) => {
      if (question.questionId == questionId) {
        editorData[meetingId][index].value = editorValue;
      }
    });

    res.send("saved");
  }
});

server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
});