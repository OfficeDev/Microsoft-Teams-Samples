const express = require('express');
const app = express();

app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

// parse application/json
app.use(express.json());

app.get('/configure', function (req, res) {
  res.render('./views/configure');
});

app.get('/tab', function (req, res) {
    res.render('./views/tab');
});

app.listen(3978, function () {
  console.log('app listening on port 3978!');
});