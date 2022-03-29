const express = require('express');
const PORT = process.env.PORT || 3978;
const app = express();
app.use(express.json());
app.use(express.urlencoded({
    extended: true
}));
const server = require('http').createServer(app);
const io = require('socket.io')(server,{cors:{origin: "*"}});
const editorData = {};

app.get('/api/editorState', function (req, res) {
  var questionId = req.url.substring(req.url.search('=')+1,req.url.search('&'));
  var meetingId = req.url.split('&meetingId=')[1];
  var latestState = {
    "value": null,
  }
  var currentData =  editorData["meetings"];

  if(currentData==undefined){
    const newMeeting = new Array();
    newMeeting.push({meetingId:meetingId,questions:[{
      questionId:"1",
      value:""
    },
    {
      questionId:"2",
      value:""
    },{
      questionId:"3",
      value:""
    }]})
    currentData = newMeeting;
    editorData["meetings"] = currentData;
    latestState.value = "";
  }
  else if(!currentData.find((meeting) => {
    if (meeting.meetingId == meetingId) {
      return true;
    }})){
      const newMeeting = currentData;
      newMeeting.push({meetingId:meetingId,questions:[{
        questionId:"1",
        value:""
      },
      {
        questionId:"2",
        value:""
      },{
        questionId:"3",
        value:""
      }]})
      currentData = newMeeting;
      editorData["meetings"] = currentData;
      latestState.value = "";
  }
  else{
    currentData.find((meeting) => {
      if (meeting.meetingId == meetingId) {
         meeting.questions.find(question=>{
            if(question.questionId == questionId){
              latestState.value = question.value;
            }
          })
      }
    });
  }
  res.send(latestState);
});

io.on("connection", (socket) => {
    socket.on("message", (message,questionId,meetingId) => {
      let currentData = editorData["meetings"];
      let currentMeeting;
      let updateindex;
      currentData.find((meeting,meetingIndex) => {
        if (meeting.meetingId == meetingId) {
          updateindex = meetingIndex;
          currentMeeting = meeting;
            meeting.questions.find((question,index)=>{
              if(question.questionId == questionId){
                currentMeeting.questions[index].value =message;
              }
            })
        }
      });
      currentData[updateindex] = currentMeeting;
      editorData["meetings"] = currentData;
      io.emit("message", message)
    })
  });

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});