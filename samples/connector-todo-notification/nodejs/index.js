const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();
const axios = require('axios');

var webhookUrl = "";

var taskList = {
    "task": [
        {
            "Title": "Sample task 1",
            "Assigned": "Alex",
            "Description": "Description for sample task 1"
        },
        {
            "Title": "Sample task 2",
            "Assigned": "Wilbur",
            "Description": "Description for sample task 2"
        }
    ]
}

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});

server.use("/Images", express.static(path.resolve(__dirname, 'Images')));

server.get('/SetupAuth', (req, res, next) => {
    res.render('./views/SetupAuth')
});

server.get('/TaskDetails', (req, res, next) => {
    res.render('./views/TaskDetails', { taskList: JSON.stringify(taskList) })
});

server.post('/Task/Save', (req, res, next) => {
    var link = process.env.BaseUrl + "/TaskDetails"

    var task = {
        "Title": req.body.title,
        "Description": req.body.description,
        "Assigned": req.body.assignedTo
    };

    taskList.task.push(task);

    var card = {
        "@type": "MessageCard",
        "summary": "Task Created",
        "sections": [
            {
                "activityTitle": "Task " + task.Title,
                "facts": [
                    {
                        "name": 'Title:',
                        "value": task.Title
                    },
                    {
                        "name": 'Description:',
                        "value": task.Description
                    },
                    {
                        "name": 'Assigned To:',
                        "value": task.Assigned
                    }
                ]
            }],
        "potentialAction": [
            {
                "@context": "http://schema.org",
                "@type": "ViewAction",
                "name": "View Task List",
                "target": [
                    process.env.BaseUrl + "/TaskDetails"
                ]
            }
        ]
    }
    
    axios.post(webhookUrl, card)
        .then(res => {
            console.log(`statusCode: ${res.status}`)
            console.log(res)
        })
        .catch(error => {
            console.error(error)
        })
});

server.get('/Create', (req, res, next) => {
    res.render('./views/Create')
});

server.get('/SimpleStart', (req, res, next) => {
    res.render('./views/SimpleStart', { clientId: JSON.stringify(process.env.MicrosoftAppId) })
});

server.get('/SimpleEnd', (req, res, next) => {
    res.render('./views/SimpleEnd', { clientId: JSON.stringify(process.env.MicrosoftAppId) })
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

// This is used to decode the id token passed from client side.
server.post('/decodeToken',(req, res)=>{
    var token = req.body.idToken;

    if(token !== null || token !== undefined){
      const base64String = token.split('.')[1];
      const decodedValue = JSON.parse(Buffer.from(base64String, 'base64').toString('ascii'));
      res.json(decodedValue);
      }
  })

server.post('/Connector/Save', (req, res) => {

    var link = process.env.BaseUrl + "/TaskDetails"
    webhookUrl = req.body.webhookUrl;
    var card = {
        "@type": "MessageCard",
        "summary": "Welcome Message",
        "sections": [{
            "activityTitle": "Welcome Message",
            "text": "Teams todo connector is setup we will notify you when new task is created [here](" + link + ")"
        }]
    }

    axios.post(req.body.webhookUrl, card)
        .then(res => {
            console.log(`statusCode: ${res.status}`)
            console.log(res)
        })
        .catch(error => {
            console.error(error)
        })

});