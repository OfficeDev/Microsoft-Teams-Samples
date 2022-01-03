const path = require('path');
const express = require('express');
const cors = require('cors');
const PORT = process.env.PORT || 3978;
const server = express();
const axios = require('axios');

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.post('/api/Send', (req, res) => {
    var cardJson = JSON.parse(req.body.cardBody);
    axios.post(req.body.webhookUrl, cardJson)
    .then(res => {
        console.log(`statusCode: ${res.status}`)
        console.log(res)
    })
    .catch(error => {
        console.error(error)
    })
});

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});