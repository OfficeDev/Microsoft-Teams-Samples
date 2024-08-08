const path = require('path');
const express = require('express');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const server = express();
server.use(express.static(path.resolve(__dirname, '../client/build')));
server.use('/api', require('./api'))

server.get('*', (req, res) => {
    res.sendFile(path.resolve(__dirname, '../client/build', 'index.html'));
});

const port = process.env.port || process.env.PORT || 3978;

server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);