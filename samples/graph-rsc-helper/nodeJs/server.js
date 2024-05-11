const express = require('express');
const bodyparser = require('body-parser');
const env = require('dotenv')
const path = require('path');
const fs = require('fs');
const auth = require('./auth');
const indexRouter = require('./routes/index');
const { v4: uuidv4 } = require('uuid');
require('isomorphic-fetch');
const axios = require('axios');

const app = express();

app.use(bodyparser.urlencoded({ extended: false }))
app.use(bodyparser.json())
app.use(express.static(__dirname + '/Styles'));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

const credential = require('./graph/credential');
const buidGraphClient = require('./graph/graphClient');
const graphClient = buidGraphClient(credential);
app.use('/', indexRouter);

app.get('/configure', function (req, res) {
  res.render('./views/configure');
});

app.get('/sendNotification', function (req, res) {
  res.render('./views/sendNotification');
});

app.get('/RSCGraphAPI', function (req, res) {
  fs.readFile('APImethods.json', 'utf8', (err, data) => {
    if (err) {
        console.error('Error reading file:', err);
        return;
    }

    try {
        // Parse JSON data
        const items = JSON.parse(data);
        res.render('./views/RSCGraphAPI',{items});
    } catch (error) {
        console.error('Error parsing JSON:', error);
    }
});
});

app.post('/sendFeedNotification', async function (req, res) {
  try {
    const tenantId = req.body.tenantId;
    console.log(req.body.url);
    console.log(req.body.requestBody);
    const responseData = await sendActivityFeedNotificationtest(tenantId, req.body.url, req.body.requestBody);
    console.log('Notification send success');
    res.json(responseData.status); 
  } catch (err) {
    console.error('Error sending feed notification:', err.message);
    res.status(500).send('Error: ' + err.message);
  }
});

app.post('/graphCall', async function (req, res) {
  try {
    const teamId = req.body.teamId;
    const result = await graphClient.api(`teams/${teamId}/channels`).get();
    res.json(JSON.stringify(result, null, 2)); 
  } catch (err) {
    console.error('Error graphCall:', err.message);
    res.status(500).send('Error: ' + err.message);
  }
});

async function sendActivityFeedNotificationtest(tenantId, url, requestBody) {
  var token = await auth.getAccessToken(tenantId);
  var postData = JSON.parse(requestBody);
  const startIndex = url.indexOf('/users/') + '/users/'.length;
  const endIndex = url.indexOf('/', startIndex);
  const recipientUserId = url.substring(startIndex, endIndex);
  const randomUUID = uuidv4();
  const encodedString = Buffer.from(recipientUserId+"##"+process.env.teamsAppId).toString('base64');
  postData.topic.value = postData.topic.value +"/"+encodedString;

  const config = {
    headers: {
      Authorization: "Bearer " + token
    }
  };
  
  var response = await axios.post(url, postData, config)
  console.log('Notification sent', response);
  return response;
}

app.listen(3978 || 3978, function () {
  console.log('app listening on port 3978!');
});