const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3978;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
    extended: true
}));
server.engine('.html', require('ejs').renderFile);
server.set('views', __dirname);
server.set('view engine', 'ejs');
server.use('/api', require('./api'));

server.get('/content', (req, res, next) => {
    res.sendFile('views/content-tab.html', {root: __dirname })
});

server.get('/tab', (req, res, next) => {
    res.render('./views/sampleTab', { microsoftAppId: process.env.MicrosoftAppId, baseUrl: process.env.BaseUrl });
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});
server.listen(PORT, () => {
    console.log(`Server listening on http://localhost:${PORT}`);
});
