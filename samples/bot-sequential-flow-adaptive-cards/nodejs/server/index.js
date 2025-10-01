const path = require('path');
const express = require('express');
const cors = require('cors');
const ENV_FILE = path.join(__dirname, '../.env');
require('dotenv').config({ path: ENV_FILE });

const server = express();

server.use(express.json());

server.use('/api', require('./api'));

server.use("/Images", express.static(path.resolve(__dirname, '../Images')));

server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

const port = process.env.port || process.env.PORT || 3978;

server.listen(port, () => {
    console.log(`\Bot/ME Server listening on http://localhost:${ port }`);
});
