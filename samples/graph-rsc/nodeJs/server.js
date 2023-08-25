const express = require('express');
const bodyparser = require('body-parser');
const env = require('dotenv')
const path = require('path');

const auth = require('./auth');
const indexRouter = require('./routes/index');
require('isomorphic-fetch');

const app = express();

app.use(express.static(__dirname + '/Styles'));
app.engine('html', require('ejs').renderFile);
app.set('view engine', 'ejs');
app.set('views', __dirname);

const ENV_FILE = path.join(__dirname, '.env');
require('dotenv').config({ path: ENV_FILE });

app.use('/',indexRouter);

  app.get('/configure', function(req, res) {
    res.render('./views/configure');
  });

  app.get('/rscdemo', function(req, res) {
    var tenantId= req.url.split('=')[1];
    auth.getAccessToken(tenantId).then(async function (token) {
     res.render('./views/rscdemo',{token:JSON.stringify(token)});
    });
  });

  app.get('/sendNotification', function(req, res) {
    var tenantId= process.env.TenantId
    auth.getAccessToken(tenantId).then(async function (token) {
     res.render('./views/sendNotification',{token:JSON.stringify(token)});
    });
  });

app.listen(3978 ||3978, function () {
  console.log('app listening on port 3978!');
});