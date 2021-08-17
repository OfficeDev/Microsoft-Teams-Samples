const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

server.use("/images", express.static(path.resolve(__dirname, '../images')));
server.use('/static', express.static(path.join(__dirname, 'static')))
server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.get('/configure', (req, res) => {
    res.sendFile('views/configure.html', {root: __dirname })
});
server.get('/conversationTab', (req, res) => {
    res.sendFile('views/conversation-tab.html', {root: __dirname })
});
server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});
