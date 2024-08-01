const express = require("express");
const https = require('https');
const bodyparser = require("body-parser");
const path = require("path");
const fs = require("fs");
const indexRouter = require("./routes/index");
require("isomorphic-fetch");
require("dotenv").config({ path: path.join(__dirname, ".env") });

const app = express();

app.use(bodyparser.urlencoded({ extended: false }));
app.use(bodyparser.json());
app.use(express.static(__dirname + "/Styles"));
app.engine("html", require("ejs").renderFile);
app.set("view engine", "ejs");
app.set("views", __dirname);

const apiClient = require("./graph/client");

app.use("/", indexRouter);

app.get("/configure", function (req, res) {
  res.render("./views/configure");
});

app.get("/RSCGraphAPI", function (req, res) {
  fs.readFile("graph/apiList.json", "utf8", (err, data) => {
    if (err) {
      console.error("Error reading file:", err);
      return;
    }

    try {
      // Parse JSON data
      const items = JSON.parse(data);
      res.render("./views/RSCGraphAPI", { items });
    } catch (error) {
      console.error("Error parsing API list JSON:", error);
    }
  });
});

app.post("/handleRequest", async function (req, res) {
  const apiId = req.body.apiId;
  const tenantId = req.body.tenantId;
  const scope = req.body.scope;
  
  try {
    console.log("apiId: ", apiId);
    if (scope === "team") {
      const result = await apiClient.callTeamAPI(req.body.url,req.body.requestType, req.body.requestBody, req.body.userId, req.body.teamId, req.body.channelId);
      res.json(JSON.stringify(result, null, 2));
    } else {
      const result = await apiClient.callChatAPI(req.body.url,req.body.requestType, req.body.requestBody, req.body.userId, req.body.chatId);
      res.json(JSON.stringify(result, null, 2));
    }
  } catch (error) {
    console.error("Error handleRequest:", error.message);
    res.status(500).send("Error: " + error.message);
  }
});

const privateKey = fs.readFileSync(process.env.SSL_KEY_FILE);
const certificate = fs.readFileSync(process.env.SSL_CRT_FILE);
const credentials = {key: privateKey, cert: certificate};
const server = https.createServer(credentials, app);

server.listen(3978, function () {
  console.log("app listening on port 3978!");
});
