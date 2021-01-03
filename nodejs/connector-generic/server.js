var express = require('express'),
  bodyParser = require('body-parser'),
  cookieParser = require('cookie-parser'),
  session = require('express-session');

var app = express();

app.set('views', __dirname + '/views');
app.set('view engine', 'jade');

app.use(express.static(__dirname + '/public'));

app.use(session({
  secret: "random",
  saveUninitialized: true,
  resave: true
}));

// This is used to prevent your tabs from being embedded in other systems than Microsoft Teams
app.use(function (req, res, next) {
  res.setHeader("Content-Security-Policy", "frame-ancestors teams.microsoft.com *.teams.microsoft.com *.skype.com");
  res.setHeader("X-Frame-Options", "ALLOW-FROM https://teams.microsoft.com/."); // IE11
  return next();
});

app.get('/connectorconfig', function (req, res) {
  renderView(req, res, 'connectorconfig.jade', { user: req.user });
});

function renderView(req, res, view, locals) {
  if (locals === undefined) {
    locals = {};
  }
  res.render(view, locals);
}

var port = process.env.port || 3978;
app.listen(port, function () {
  console.log('Listening on http://localhost:' + port);
});