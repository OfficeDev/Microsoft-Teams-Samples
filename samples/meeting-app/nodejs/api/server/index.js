const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

const candidateRepo = require('./data/candidate')

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));

server.use('/api', require('./api'));

server.get('/api/candidate', async (req, res) => {
    candidateRepo.getCandidateDetails(function(response) {
        res.send(response);
    });
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


