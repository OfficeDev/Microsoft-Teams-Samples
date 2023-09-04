const path = require('path');
const express = require('express');

const server = express();
server.use("/images", express.static(path.resolve(__dirname, '../images')));
server.use('/static', express.static(path.join(__dirname, 'static')))

server.get('/configure', (req, res) => {
    res.sendFile('views/configure.html', {root: __dirname })
});
server.get('/conversationTab', (req, res) => {
    res.sendFile('views/conversation-tab.html', {root: __dirname })
});
server.get('*', (req, res) => {
    res.json({ error: 'Route not found' });
});

const port = process.env.port || process.env.PORT || 3978;

server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);