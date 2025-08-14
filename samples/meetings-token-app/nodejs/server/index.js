const path = require('path');
const express = require("express");
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const PORT = process.env.PORT || 3000;
const server = express();

server.use(cors());
server.use(express.json());
server.use(express.urlencoded({
  extended: true
}));
server.use(express.static(path.resolve(__dirname, '../client/build')));
server.use('/api', require('./api'))


server.get('*', (req, res) => {
    res.sendFile(path.resolve(__dirname, '../client/build', 'index.html'));
});

server.listen(PORT, () => {
  console.log(`Server listening on http://localhost:${PORT}`);
});