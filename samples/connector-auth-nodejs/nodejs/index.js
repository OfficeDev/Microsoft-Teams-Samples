const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });
const PORT = process.env.PORT || 3978;
const server = express();

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

server.get('/SimpleStart', (req, res, next) => {
    res.render('./views/SimpleStart')
});

server.get('/SimpleEnd', (req, res, next) => {
    res.render('./views/SimpleEnd')
});

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});