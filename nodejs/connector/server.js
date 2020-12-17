var express = require('express'),
  rest = require('restler'),
  bodyParser = require('body-parser'),
  cookieParser = require('cookie-parser'),
  session = require('express-session'),
  async = require('async'),
  passport = require('passport'),
  GitHubStrategy = require('passport-github2').Strategy,
  Config = require('./models/config.js'),
  utils = require('./utils/util.js'),
  mongoose = require('mongoose'),
  uuid = require('node-uuid'),
  User = require('./models/user.js'),
  gitConfig = require('config'),
  XMLHttpRequest = require("xmlhttprequest").XMLHttpRequest

// Database to store information about the WebhookUrls
mongoose.connect('mongodb://goelashish7:goelashish7@ds062059.mlab.com:62059/gitauth');
var serviceUrl = gitConfig.get('github.serviceUrl');

passport.serializeUser(function (user, done) {
  done(null, user);
});

passport.deserializeUser(function (user, done) {
  done(null, user);
});

passport.use(new GitHubStrategy({
  clientID: gitConfig.get('github.clientId'),
  clientSecret: gitConfig.get('github.clientSecret'),
  callbackURL: gitConfig.get('github.callbackUrl')
},
  function (accessToken, refreshToken, profile, done) {
    var user = new User();
    user.userid = profile.id;
    user.username = profile.username;
    user.displayName = profile.displayName;
    user.accessToken = accessToken;
    done(null, user);
  }));

var app = express();
var webhook_url;
var group_name;
var session;
app.set('views', __dirname + '/views');
app.set('view engine', 'jade');

app.use(express.static(__dirname + '/public'));
app.use(cookieParser());

app.use(bodyParser.json());

app.use(bodyParser.urlencoded({
  extended: true
}));

app.use(session({
  secret: gitConfig.get('session.secret'),
  saveUninitialized: true,
  resave: true
}));

app.use(passport.initialize());
app.use(passport.session());

app.get('/auth/github', function (req, res, next) {
  passport.authenticate('github', {
    scope: ['public_repo'],
    callbackURL: gitConfig.get('github.callbackUrl'),
    display: 'popup'
  })(req, res, next);
});

app.get('/auth/github/callback', function (req, res, next) {
  passport.authenticate('github', {
    successRedirect: '/gitconfig',
    failureRedirect: '/signin',
    callbackURL: gitConfig.get('github.callbackUrl')
  })(req, res, next);
});

app.get('/signin', function (req, res) {
  renderView(req, res, 'signin.jade', { serviceUrl: serviceUrl });
});

app.get('/config', function (req, res) {
  session = req.session;
  session.webhook_url = req.query.webhook_url;
  session.group_name = req.query.group_name;
  renderView(req, res, 'githubsignin.jade');
});

app.get('/gitconfig', function (req, res) {
  renderView(req, res, 'config.jade', { user: req.user });
});

app.post('/config', function (req, res) {
  var repo_name = req.body.selectpicker;
  var webhook_url = req.session.webhook_url;
  var group_name = req.session.group_name;

  findOrCreateConfig(group_name, webhook_url, repo_name, function (error, configFound) {
    //register the webhook to the service(for e.g. github here) to get notification when any event happens
    registerWebhook(configFound, req.user);
  });

  // Generate connector welcome message and post it to the team
  var message = utils.createWelcomeMessage(repo_name);
  rest.postJson(webhook_url, message).on('complete', function (data, response) {
    renderView(req, res, 'close.jade');
  });

});

app.post('/comment', (req, res) => {
  //setHeader with CARD-ACTION-STATUS will post the message to the same conversation
  //The comments entered is posted back in the conversation here
  res.setHeader("CARD-ACTION-STATUS", "Your comments " + "**-- " + req.body.comment + " --**" + " are posted succesfully");
  res.sendStatus(200);
});

app.post('/mergerequest', (req, res) => {
  //setHeader with CARD-ACTION-STATUS will post the message to the same conversation
  res.setHeader("CARD-ACTION-STATUS", "Your pull request is merged succesfully");
  res.sendStatus(200);
});

app.post('/closerequest', (req, res) => {
  //setHeader with CARD-ACTION-STATUS will post the message to the same conversation
  res.setHeader("CARD-ACTION-STATUS", "Your pull request is closed succesfully");
  res.sendStatus(200);
});

// This is the endpoint registered at the github which sends notification if any changes happen in the repo(i.e. pull requests open/merge/close)
app.post('/notify', (req, res) => {
  var guid = req.query.id;
  var payload = req.body;
  if (payload.pull_request) {
    getGitConfiguration(guid, function (error, configFound) {
      if (configFound) {
        sendConnectorCard(configFound, payload);
        res.sendStatus(200);
      }
    });
  }
});

//creates a card and send it to the channel connector is configured for
function sendConnectorCard(config, payload) {
  var message = utils.createNotificationCard(payload, serviceUrl);
  rest.postJson(config.webhookUrl, message).on('complete', function (data, response) {
    console.log("success");
  });
}

app.get('/close', function (req, res) {
  renderView(req, res, 'close.jade');
});

function renderView(req, res, view, locals) {
  if (locals === undefined) {
    locals = {};
  }
  res.render(view, locals);
}

function findOrCreateConfig(group_name, webhook_url, repo_name, done) {
  var uuidGit = uuid.v1();

  Config.findOne({ 'guid': uuidGit }, function (err, config) {
    if (err)
      return done(err);
    if (config)
      return done(null, config);
    else {
      var config = new Config();
      config.guid = uuidGit;
      config.groupName = group_name;
      config.webhookUrl = webhook_url;
      config.repoName = repo_name;
      config.save(function (err) {
        if (err)
          return done(err);
      });
      return done(null, config);
    }
  });
}

function getGitConfiguration(guid, done) {
  Config.findOne({ 'guid': guid }, function (err, config) {
    if (config)
      return done(null, config);
    else
      return done(err);
  });
}

function registerWebhook(configuration, user) {
  var message =
    {
      "name": "web",
      "active": true,
      "events": [
        "push",
        "pull_request"
      ],
      "config": {
        "url": serviceUrl + "/notify?id=" + configuration.guid,
        "content_type": "json"
      }
    };

  var xmlHttp = new XMLHttpRequest();
  var url = "https://api.github.com/repos/" + user.username + "/" + configuration.repoName + "/" + "hooks";
  var authorization = "Bearer " + user.accessToken;
  xmlHttp.open("POST", url, false);
  xmlHttp.setRequestHeader("Content-type", "application/json");
  xmlHttp.setRequestHeader('Authorization', authorization);
  xmlHttp.send(JSON.stringify(message));
  var result = JSON.parse(xmlHttp.responseText);
  console.log(result);
}

function getConfig(group_name, done) {
  Config.findOne({ 'groupName': group_name }, function (err, config) {
    if (err) {
      done(err);
    }
    else
      done(null, config);
  });
}

var port = process.env.port || 3000;
app.listen(port, function () {
  console.log('Listening on http://localhost:' + port);
});