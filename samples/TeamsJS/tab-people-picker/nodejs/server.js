const express = require('express');

const server = express();
server.engine('html', require('ejs').renderFile);
server.set('view engine', 'ejs');
server.set('views', __dirname);

server.get('/configure', function (req, res) {
  res.render('./views/configure');
});

server.get('/tab', function (req, res) {
    res.render('./views/tab');
});

const port = process.env.port || process.env.PORT || 3978;

server.listen(port, () => 
    console.log(`\Bot/ME service listening at http://localhost:${port}`)
);