const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

const httpProxy = require('http-proxy');
const proxy = httpProxy.createServer({});

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.use('/api', require('./api'));

server.get('/configure', (req, res) => {
    res.setHeader('Content-Security-Policy', 'frame-src blob: data: https: mailto: ms-appx-web: ms-excel: ms-powerpoint: ms-visio: ms-word: onenote: pdf: local.teams.office.com:* local.teams.live.com:* localhost:* msteams: sip: sips: ms-whiteboard-preview:');
    res.redirect('http://localhost:3000/configure');
});

server.get('/details', (req, res) => {
    proxy.web(req, res, { target: 'http://localhost:3000/details' });
});

server.get('/api/candidate', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Question/insertQuest', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Question/edit:meetingId', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Question/delete', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Feedback', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Notes:email', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Notes', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Candidate/file', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.get('/api/Notify', (req, res) => {
    res.json({ error: 'Route not found' });
});

server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});


